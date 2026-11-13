using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TurboRent.BuildingBlocks.Http;
using TurboRent.Modules.Catalog.Domain;
using TurboRent.Modules.Catalog.Features.Shared;
using TurboRent.Modules.Catalog.Infrastructure.Persistence;
using TurboRent.Modules.Identity.Contracts;
using static TurboRent.BuildingBlocks.Http.ApiProblem;

namespace TurboRent.Modules.Catalog.Features.Bookings;

/// <summary>
/// Booking requests. A renter asks for dates, the owner confirms or declines. Confirmed bookings
/// block those days; payment is arranged directly between renter and owner.
/// </summary>
public static class BookingEndpoints
{
    /// <summary>Open requests a renter may have at once, to stop request spam.</summary>
    public const int MaxOpenRequestsPerRenter = 5;
    public const int MaxDaysAhead = 365;

    public static IEndpointRouteBuilder MapBookings(this IEndpointRouteBuilder endpoints)
    {
        var renter = endpoints.MapGroup("/api/v1/bookings").WithTags("Bookings");
        renter.MapPost("", CreateAsync).RequireRateLimiting(RateLimitPolicies.Writes);
        renter.MapGet("", RenterListAsync).RequireRateLimiting(RateLimitPolicies.Dashboard);
        renter.MapPost("/{id:guid}/cancel", CancelAsync).RequireRateLimiting(RateLimitPolicies.Writes);

        var owner = endpoints.MapGroup("/api/v1/owner/bookings").WithTags("Owner bookings");
        owner.MapGet("", OwnerListAsync).RequireRateLimiting(RateLimitPolicies.Dashboard);
        owner.MapPost("/{id:guid}/confirm", ConfirmAsync).RequireRateLimiting(RateLimitPolicies.Writes);
        owner.MapPost("/{id:guid}/decline", DeclineAsync).RequireRateLimiting(RateLimitPolicies.Writes);
        return endpoints;
    }

    private static async Task<IResult> CreateAsync(CreateBookingRequest request, HttpContext http,
        IAntiforgery antiforgery, IAuthenticatedUser users, CatalogDbContext db, IUserNotifier notifier,
        TimeProvider time, CancellationToken cancellationToken)
    {
        var (user, denied) = await AuthorizeWriteAsync(http, antiforgery, users, cancellationToken);
        if (denied is not null) return denied;
        var today = BakuCalendar.Today(time);
        if (request.ListingId == Guid.Empty || request.StartDate is not { } start || request.EndDate is not { } end ||
            start < today || end < start || start > today.AddDays(MaxDaysAhead) ||
            end.DayNumber - start.DayNumber + 1 > Booking.MaxDays ||
            request.Message is { Length: > Booking.MaxMessageLength })
            return Error(400, "invalid_booking_dates");

        var listing = await db.Listings.AsNoTracking()
            .Where(item => item.Id == request.ListingId && item.Status == ListingStatus.Approved)
            .Select(item => new { item.Id, item.OwnerUserId, item.DailyPrice })
            .SingleOrDefaultAsync(cancellationToken);
        if (listing?.OwnerUserId is not { } ownerId) return Results.NotFound();
        if (ownerId == user!.UserId) return Error(400, "cannot_book_own_listing");

        var open = await db.Bookings.CountAsync(booking => booking.RenterUserId == user.UserId &&
            booking.Status == BookingStatus.Requested, cancellationToken);
        if (open >= MaxOpenRequestsPerRenter) return Error(409, "too_many_open_requests");
        if (await HasConfirmedOverlapAsync(db, listing.Id, start, end, excluding: null, cancellationToken))
            return Error(409, "dates_unavailable");

        var booking = Booking.Request(listing.Id, user.UserId, ownerId, start, end, listing.DailyPrice,
            request.Message, DateTimeOffset.UtcNow);
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(cancellationToken);
        await notifier.NotifyAsync(ownerId, new UserNotification(UserNotificationKind.BookingRequested,
            await VehicleNameAsync(db, listing.Id, cancellationToken), Range(booking), "/owner/bookings"),
            cancellationToken);
        return Results.Created($"/api/v1/bookings/{booking.Id}", ToResponse(booking, null, null));
    }

    private static async Task<IResult> RenterListAsync(string? locale, HttpContext http, IAuthenticatedUser users,
        CatalogDbContext db, IListingOwnerDirectory directory, CancellationToken cancellationToken)
    {
        NoStore(http);
        if (!http.Request.IsHttps) return Error(403, "https_required");
        var user = await users.FindAsync(http, cancellationToken);
        if (user is null) return Error(401, "not_authenticated");
        var bookings = await db.Bookings.AsNoTracking().Where(booking => booking.RenterUserId == user.UserId)
            .OrderByDescending(booking => booking.CreatedAtUtc).Take(100).ToListAsync(cancellationToken);
        var vehicles = await VehicleNamesAsync(db, bookings.Select(b => b.ListingId), locale, cancellationToken);
        var owners = await directory.FindAsync(bookings.Select(b => b.OwnerUserId).Distinct().ToArray(), cancellationToken);
        return Results.Ok(new
        {
            items = bookings.Select(booking =>
            {
                owners.TryGetValue(booking.OwnerUserId, out var owner);
                // The owner's phone is shared with the renter once the booking is confirmed.
                var contact = owner is null ? null : new
                {
                    name = owner.CompanyName ?? owner.DisplayName,
                    phone = booking.Status == BookingStatus.Confirmed ? owner.ContactPhone : null
                };
                return ToResponse(booking, vehicles.GetValueOrDefault(booking.ListingId), contact);
            })
        });
    }

    private static async Task<IResult> OwnerListAsync(string? locale, HttpContext http, IVerifiedListingOwner owners,
        CatalogDbContext db, IListingOwnerDirectory directory, CancellationToken cancellationToken)
    {
        NoStore(http);
        if (!http.Request.IsHttps) return Error(403, "https_required");
        var access = await owners.FindListingOwnerAccessAsync(http, cancellationToken);
        if (access is null) return Error(401, "owner_sign_in_required");
        if (!access.HasOwnerProfile) return Error(403, "owner_account_required");
        var bookings = await db.Bookings.AsNoTracking().Where(booking => booking.OwnerUserId == access.UserId)
            .OrderBy(booking => booking.Status == BookingStatus.Requested ? 0 : 1)
            .ThenByDescending(booking => booking.StartDate).Take(200).ToListAsync(cancellationToken);
        var vehicles = await VehicleNamesAsync(db, bookings.Select(b => b.ListingId), locale, cancellationToken);
        var renters = await directory.FindAsync(bookings.Select(b => b.RenterUserId).Distinct().ToArray(), cancellationToken);
        return Results.Ok(new
        {
            items = bookings.Select(booking =>
            {
                renters.TryGetValue(booking.RenterUserId, out var renter);
                // The renter's email is shared with the owner only after the owner confirms.
                var contact = renter is null ? null : new
                {
                    name = renter.DisplayName,
                    email = booking.Status == BookingStatus.Confirmed ? renter.Email : null
                };
                return ToResponse(booking, vehicles.GetValueOrDefault(booking.ListingId), contact);
            })
        });
    }

    private static async Task<IResult> CancelAsync(Guid id, HttpContext http, IAntiforgery antiforgery,
        IAuthenticatedUser users, CatalogDbContext db, IUserNotifier notifier, TimeProvider time,
        CancellationToken cancellationToken)
    {
        var (user, denied) = await AuthorizeWriteAsync(http, antiforgery, users, cancellationToken);
        if (denied is not null) return denied;
        var booking = await db.Bookings.SingleOrDefaultAsync(
            item => item.Id == id && item.RenterUserId == user!.UserId, cancellationToken);
        if (booking is null) return Results.NotFound();
        if (booking.Status is not (BookingStatus.Requested or BookingStatus.Confirmed) ||
            booking.StartDate <= BakuCalendar.Today(time))
            return Error(409, "booking_not_cancellable");
        booking.Cancel(DateTimeOffset.UtcNow);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Error(409, "booking_changed"); }
        await notifier.NotifyAsync(booking.OwnerUserId, new UserNotification(UserNotificationKind.BookingCancelled,
            await VehicleNameAsync(db, booking.ListingId, cancellationToken), Range(booking), "/owner/bookings"),
            cancellationToken);
        return Results.Ok(ToResponse(booking, null, null));
    }

    private static async Task<IResult> ConfirmAsync(Guid id, HttpContext http, IAntiforgery antiforgery,
        IVerifiedListingOwner owners, CatalogDbContext db, IUserNotifier notifier, TimeProvider time,
        CancellationToken cancellationToken)
    {
        var (ownerId, denied) = await AuthorizeOwnerWriteAsync(http, antiforgery, owners, cancellationToken);
        if (denied is not null) return denied;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var booking = await db.Bookings.SingleOrDefaultAsync(
            item => item.Id == id && item.OwnerUserId == ownerId, cancellationToken);
        if (booking is null) return Results.NotFound();
        if (booking.Status != BookingStatus.Requested || booking.StartDate < BakuCalendar.Today(time))
            return Error(409, "booking_not_pending");

        // Serialize confirmations per listing so two overlapping requests can never both be confirmed.
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({ListingLockKey(booking.ListingId)})", cancellationToken);
        if (await HasConfirmedOverlapAsync(db, booking.ListingId, booking.StartDate, booking.EndDate,
                excluding: booking.Id, cancellationToken))
            return Error(409, "dates_unavailable");

        var now = DateTimeOffset.UtcNow;
        booking.Confirm(now);
        // Other requests for the same days can no longer be fulfilled.
        var conflicting = await db.Bookings
            .Where(item => item.ListingId == booking.ListingId && item.Id != booking.Id &&
                item.Status == BookingStatus.Requested &&
                item.StartDate <= booking.EndDate && item.EndDate >= booking.StartDate)
            .ToListAsync(cancellationToken);
        foreach (var other in conflicting) other.Decline(now);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException) { return Error(409, "booking_changed"); }

        var vehicle = await VehicleNameAsync(db, booking.ListingId, cancellationToken);
        await notifier.NotifyAsync(booking.RenterUserId, new UserNotification(UserNotificationKind.BookingConfirmed,
            vehicle, Range(booking), "/account"), cancellationToken);
        foreach (var other in conflicting)
            await notifier.NotifyAsync(other.RenterUserId, new UserNotification(UserNotificationKind.BookingDeclined,
                vehicle, Range(other), "/account"), cancellationToken);
        return Results.Ok(ToResponse(booking, null, null));
    }

    private static async Task<IResult> DeclineAsync(Guid id, HttpContext http, IAntiforgery antiforgery,
        IVerifiedListingOwner owners, CatalogDbContext db, IUserNotifier notifier, CancellationToken cancellationToken)
    {
        var (ownerId, denied) = await AuthorizeOwnerWriteAsync(http, antiforgery, owners, cancellationToken);
        if (denied is not null) return denied;
        var booking = await db.Bookings.SingleOrDefaultAsync(
            item => item.Id == id && item.OwnerUserId == ownerId, cancellationToken);
        if (booking is null) return Results.NotFound();
        if (booking.Status != BookingStatus.Requested) return Error(409, "booking_not_pending");
        booking.Decline(DateTimeOffset.UtcNow);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Error(409, "booking_changed"); }
        await notifier.NotifyAsync(booking.RenterUserId, new UserNotification(UserNotificationKind.BookingDeclined,
            await VehicleNameAsync(db, booking.ListingId, cancellationToken), Range(booking), "/account"),
            cancellationToken);
        return Results.Ok(ToResponse(booking, null, null));
    }

    private static Task<bool> HasConfirmedOverlapAsync(CatalogDbContext db, Guid listingId, DateOnly start,
        DateOnly end, Guid? excluding, CancellationToken cancellationToken) =>
        db.Bookings.AnyAsync(booking => booking.ListingId == listingId && booking.Id != excluding &&
            booking.Status == BookingStatus.Confirmed && booking.StartDate <= end && booking.EndDate >= start,
            cancellationToken);

    /// <summary>Stable 64-bit key for a PostgreSQL advisory lock derived from the listing id.</summary>
    public static long ListingLockKey(Guid listingId) => BitConverter.ToInt64(listingId.ToByteArray(), 0);

    private static async Task<(AuthenticatedUser? User, IResult? Denied)> AuthorizeWriteAsync(HttpContext http,
        IAntiforgery antiforgery, IAuthenticatedUser users, CancellationToken cancellationToken)
    {
        NoStore(http);
        if (!http.Request.IsHttps) return (null, Error(403, "https_required"));
        try { await antiforgery.ValidateRequestAsync(http); }
        catch (AntiforgeryValidationException) { return (null, Error(400, "invalid_csrf")); }
        var user = await users.FindAsync(http, cancellationToken);
        return user is null ? (null, Error(401, "not_authenticated")) : (user, null);
    }

    private static async Task<(Guid OwnerId, IResult? Denied)> AuthorizeOwnerWriteAsync(HttpContext http,
        IAntiforgery antiforgery, IVerifiedListingOwner owners, CancellationToken cancellationToken)
    {
        NoStore(http);
        if (!http.Request.IsHttps) return (Guid.Empty, Error(403, "https_required"));
        try { await antiforgery.ValidateRequestAsync(http); }
        catch (AntiforgeryValidationException) { return (Guid.Empty, Error(400, "invalid_csrf")); }
        var access = await owners.FindListingOwnerAccessAsync(http, cancellationToken);
        if (access is null) return (Guid.Empty, Error(401, "owner_sign_in_required"));
        if (!access.HasOwnerProfile) return (Guid.Empty, Error(403, "owner_account_required"));
        return (access.UserId, null);
    }

    private static async Task<string> VehicleNameAsync(CatalogDbContext db, Guid listingId,
        CancellationToken cancellationToken) =>
        (await VehicleNamesAsync(db, [listingId], "en", cancellationToken)).GetValueOrDefault(listingId)?.Name ?? "TurboRent";

    private static async Task<Dictionary<Guid, VehicleInfo>> VehicleNamesAsync(CatalogDbContext db,
        IEnumerable<Guid> listingIds, string? locale, CancellationToken cancellationToken)
    {
        var ids = listingIds.Distinct().ToArray();
        var rows = await db.Listings.AsNoTracking().Where(listing => ids.Contains(listing.Id))
            .Join(db.Models.AsNoTracking(), listing => listing.ModelId, model => model.Id,
                (listing, model) => new { listing, model })
            .Join(db.Makes.AsNoTracking(), row => row.model.MakeId, make => make.Id,
                (row, make) => new
                {
                    row.listing.Id,
                    Make = locale == "en" ? make.NameEn : make.NameAz,
                    Model = locale == "en" ? row.model.NameEn : row.model.NameAz,
                    row.listing.Year, row.listing.City, row.listing.Status
                })
            .ToListAsync(cancellationToken);
        var firstImages = await db.ListingImages.AsNoTracking()
            .Where(image => ids.Contains(image.ListingId) && image.Position == 0)
            .Select(image => new { image.ListingId, image.Id })
            .ToDictionaryAsync(image => image.ListingId, image => image.Id, cancellationToken);
        return rows.ToDictionary(row => row.Id, row => new VehicleInfo(
            $"{row.Year} {row.Make} {row.Model}", row.City,
            row.Status == ListingStatus.Approved && firstImages.TryGetValue(row.Id, out var imageId)
                ? CatalogImages.PublicUrl(row.Id, imageId) : null));
    }

    private static string Range(Booking booking) =>
        $"{booking.StartDate:yyyy-MM-dd} → {booking.EndDate:yyyy-MM-dd} ({booking.Days}) · {booking.TotalPrice:0.##} AZN";

    private static object ToResponse(Booking booking, VehicleInfo? vehicle, object? contact) => new
    {
        booking.Id, booking.ListingId, booking.StartDate, booking.EndDate, booking.Days,
        booking.DailyPrice, booking.TotalPrice, booking.Status, booking.Message, booking.CreatedAtUtc,
        booking.DecidedAtUtc, Vehicle = vehicle?.Name, City = vehicle?.City, Image = vehicle?.Image, Contact = contact
    };

    private sealed record VehicleInfo(string Name, string City, string? Image);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record CreateBookingRequest(Guid ListingId, DateOnly? StartDate, DateOnly? EndDate, string? Message);
}
