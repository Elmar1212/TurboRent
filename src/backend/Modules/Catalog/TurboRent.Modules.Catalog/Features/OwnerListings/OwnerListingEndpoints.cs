using System.Globalization;
using System.Text.Json.Serialization;
using Amazon.S3;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurboRent.BuildingBlocks.Http;
using TurboRent.Modules.Catalog.Domain;
using TurboRent.Modules.Catalog.Features.Shared;
using TurboRent.Modules.Catalog.Infrastructure.Persistence;
using TurboRent.Modules.Catalog.Infrastructure.Storage;
using TurboRent.Modules.Identity.Contracts;
using static TurboRent.BuildingBlocks.Http.ApiProblem;

namespace TurboRent.Modules.Catalog.Features.OwnerListings;

/// <summary>Listing management for approved owner and company accounts.</summary>
public static class OwnerListingEndpoints
{
    private const long MaxRequestBytes = ListingRules.MaxTotalImageBytes + 1024 * 1024;

    public static IEndpointRouteBuilder MapOwnerListings(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/owner/listings").WithTags("Owner listings");
        group.MapPost("", CreateAsync).RequireRateLimiting(RateLimitPolicies.Writes);
        group.MapGet("", ListAsync).RequireRateLimiting(RateLimitPolicies.Dashboard);
        group.MapGet("/{id:guid}", GetAsync).RequireRateLimiting(RateLimitPolicies.Dashboard);
        group.MapPut("/{id:guid}", UpdateAsync).RequireRateLimiting(RateLimitPolicies.Writes);
        group.MapPost("/{id:guid}/archive", ArchiveAsync).RequireRateLimiting(RateLimitPolicies.Writes);
        group.MapPost("/{id:guid}/restore", RestoreAsync).RequireRateLimiting(RateLimitPolicies.Writes);
        group.MapDelete("/{id:guid}", DeleteAsync).RequireRateLimiting(RateLimitPolicies.Writes);
        group.MapGet("/{id:guid}/images/{imageId:guid}", ImageAsync).RequireRateLimiting(RateLimitPolicies.Dashboard);
        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        HttpContext http,
        IAntiforgery antiforgery,
        IVerifiedListingOwner listingOwner,
        CatalogDbContext db,
        IVehicleImageStorage storage,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        NoStore(http);
        if (!http.Request.IsHttps) return Error(403, "https_required");
        var requestSize = http.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (requestSize is { IsReadOnly: false }) requestSize.MaxRequestBodySize = MaxRequestBytes;
        if (http.Request.ContentLength is > MaxRequestBytes)
            return Error(413, "request_too_large");
        var (ownerId, denied) = await AuthorizeWriteAsync(http, antiforgery, listingOwner, cancellationToken);
        if (denied is not null) return denied;

        IFormCollection form;
        try { form = await http.Request.ReadFormAsync(cancellationToken); }
        catch (InvalidDataException) { return Error(400, "invalid_listing"); }
        catch (BadHttpRequestException) { return Error(413, "request_too_large"); }

        var draft = ParseForm(form);
        if (draft is null || form.Files.Count is < 1 or > ListingRules.MaxImages)
            return Error(400, "invalid_listing");
        if (form.Files.Sum(file => file.Length) > ListingRules.MaxTotalImageBytes)
            return Error(413, "request_too_large");
        if (!await db.Models.AnyAsync(model => model.Id == draft.ModelId, cancellationToken))
            return Error(400, "invalid_listing");

        var images = new List<CleanImage>(form.Files.Count);
        foreach (var file in form.Files)
        {
            if (file.Length is < 1 or > ListingRules.MaxImageBytes) return Error(400, "invalid_image");
            var bytes = new byte[file.Length];
            await using (var input = file.OpenReadStream())
                await input.ReadExactlyAsync(bytes, cancellationToken);
            var contentType = ImageSanitizer.DetectContentType(bytes);
            var clean = contentType is null ? null : ImageSanitizer.Sanitize(bytes, contentType);
            if (clean is null) return Error(400, "invalid_image");
            images.Add(new CleanImage(clean, contentType!));
        }

        var listing = new VehicleListing
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerId,
            Status = ListingStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        ListingRules.Apply(listing, draft);
        var logger = loggerFactory.CreateLogger("OwnerListingUploads");
        var savedKeys = new List<string>(images.Count);
        for (var index = 0; index < images.Count; index++)
        {
            var metadata = new VehicleListingImage
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                ObjectKey = $"{listing.Id:N}/{Guid.NewGuid():N}",
                ContentType = images[index].ContentType,
                Length = images[index].Bytes.Length,
                Position = (short)index
            };
            try
            {
                await using var stream = new MemoryStream(images[index].Bytes, writable: false);
                await storage.PutAsync(metadata.ObjectKey, stream, metadata.ContentType, cancellationToken);
                savedKeys.Add(metadata.ObjectKey);
            }
            catch (Exception exception) when (exception is AmazonS3Exception or HttpRequestException)
            {
                logger.LogWarning(exception, "Vehicle image storage failed. ListingId: {ListingId}", listing.Id);
                await DeleteObjectsAsync(storage, savedKeys, listing.Id, logger);
                return Error(503, "image_storage_unavailable");
            }
            listing.Images.Add(metadata);
        }

        db.Listings.Add(listing);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            await DeleteObjectsAsync(storage, savedKeys, listing.Id, logger);
            throw;
        }
        return Results.Created($"/api/v1/owner/listings/{listing.Id}",
            new { id = listing.Id, status = listing.Status, imageCount = listing.Images.Count });
    }

    private static async Task<IResult> ListAsync(string? locale, HttpContext http,
        IVerifiedListingOwner listingOwner, CatalogDbContext db, CancellationToken cancellationToken)
    {
        NoStore(http);
        var (ownerId, denied) = await AuthorizeReadAsync(http, listingOwner, cancellationToken);
        if (denied is not null) return denied;
        var rows = await OwnerQuery(db, ownerId, locale)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(200)
            .ToListAsync(cancellationToken);
        return Results.Ok(new { items = await DecorateAsync(db, rows, cancellationToken) });
    }

    private static async Task<IResult> GetAsync(Guid id, string? locale, HttpContext http,
        IVerifiedListingOwner listingOwner, CatalogDbContext db, CancellationToken cancellationToken)
    {
        NoStore(http);
        var (ownerId, denied) = await AuthorizeReadAsync(http, listingOwner, cancellationToken);
        if (denied is not null) return denied;
        var row = await OwnerQuery(db, ownerId, locale).Where(item => item.Id == id).SingleOrDefaultAsync(cancellationToken);
        if (row is null) return Results.NotFound();
        return Results.Ok((await DecorateAsync(db, [row], cancellationToken))[0]);
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdateListingRequest request, HttpContext http,
        IAntiforgery antiforgery, IVerifiedListingOwner listingOwner, CatalogDbContext db,
        CancellationToken cancellationToken)
    {
        NoStore(http);
        var (ownerId, denied) = await AuthorizeWriteAsync(http, antiforgery, listingOwner, cancellationToken);
        if (denied is not null) return denied;
        var draft = ListingRules.Validate(request.ModelId, request.Year, request.City, request.DailyPrice,
            request.Transmission, request.FuelType, request.Seats, request.DescriptionAz, request.DescriptionEn);
        if (draft is null || !await db.Models.AnyAsync(model => model.Id == draft.ModelId, cancellationToken))
            return Error(400, "invalid_listing");
        var listing = await db.Listings.SingleOrDefaultAsync(
            item => item.Id == id && item.OwnerUserId == ownerId, cancellationToken);
        if (listing is null) return Results.NotFound();
        if (listing.Status is ListingStatus.Archived or ListingStatus.Suspended)
            return Error(409, "listing_not_editable");
        ListingRules.Apply(listing, draft);
        // Every edit is reviewed again before it is public, so approved details cannot be swapped.
        listing.Status = ListingStatus.Pending;
        listing.UpdatedAtUtc = DateTimeOffset.UtcNow;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Error(409, "listing_changed"); }
        return Results.Ok(new { id = listing.Id, status = listing.Status });
    }

    private static Task<IResult> ArchiveAsync(Guid id, HttpContext http, IAntiforgery antiforgery,
        IVerifiedListingOwner listingOwner, CatalogDbContext db, CancellationToken cancellationToken) =>
        ChangeStatusAsync(id, http, antiforgery, listingOwner, db, cancellationToken,
            from: [ListingStatus.Pending, ListingStatus.Approved, ListingStatus.Rejected], to: ListingStatus.Archived);

    private static Task<IResult> RestoreAsync(Guid id, HttpContext http, IAntiforgery antiforgery,
        IVerifiedListingOwner listingOwner, CatalogDbContext db, CancellationToken cancellationToken) =>
        ChangeStatusAsync(id, http, antiforgery, listingOwner, db, cancellationToken,
            from: [ListingStatus.Archived], to: ListingStatus.Pending);

    private static async Task<IResult> ChangeStatusAsync(Guid id, HttpContext http, IAntiforgery antiforgery,
        IVerifiedListingOwner listingOwner, CatalogDbContext db, CancellationToken cancellationToken,
        string[] from, string to)
    {
        NoStore(http);
        var (ownerId, denied) = await AuthorizeWriteAsync(http, antiforgery, listingOwner, cancellationToken);
        if (denied is not null) return denied;
        var listing = await db.Listings.SingleOrDefaultAsync(
            item => item.Id == id && item.OwnerUserId == ownerId, cancellationToken);
        if (listing is null) return Results.NotFound();
        if (!from.Contains(listing.Status)) return Error(409, "listing_status_conflict");
        listing.Status = to;
        listing.UpdatedAtUtc = DateTimeOffset.UtcNow;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Error(409, "listing_changed"); }
        return Results.Ok(new { id = listing.Id, status = listing.Status });
    }

    private static async Task<IResult> DeleteAsync(Guid id, HttpContext http, IAntiforgery antiforgery,
        IVerifiedListingOwner listingOwner, CatalogDbContext db, IVehicleImageStorage storage,
        ILoggerFactory loggerFactory, CancellationToken cancellationToken)
    {
        NoStore(http);
        var (ownerId, denied) = await AuthorizeWriteAsync(http, antiforgery, listingOwner, cancellationToken);
        if (denied is not null) return denied;
        var listing = await db.Listings.Include(item => item.Images).SingleOrDefaultAsync(
            item => item.Id == id && item.OwnerUserId == ownerId, cancellationToken);
        if (listing is null) return Results.NotFound();
        // Booking history belongs to renters too; such listings can only be archived.
        if (await db.Bookings.AnyAsync(booking => booking.ListingId == id, cancellationToken))
            return Error(409, "listing_has_bookings");
        var keys = listing.Images.Select(image => image.ObjectKey).ToList();
        db.Listings.Remove(listing);
        await db.SaveChangesAsync(cancellationToken);
        await DeleteObjectsAsync(storage, keys, id, loggerFactory.CreateLogger("OwnerListingUploads"));
        return Results.NoContent();
    }

    private static async Task<IResult> ImageAsync(Guid id, Guid imageId, HttpContext http,
        IVerifiedListingOwner listingOwner, CatalogDbContext db, IVehicleImageStorage storage,
        CancellationToken cancellationToken)
    {
        var (ownerId, denied) = await AuthorizeReadAsync(http, listingOwner, cancellationToken);
        if (denied is not null) return denied;
        var metadata = await db.ListingImages.AsNoTracking()
            .Where(image => image.Id == imageId && image.ListingId == id)
            .Join(db.Listings.AsNoTracking().Where(listing => listing.OwnerUserId == ownerId),
                image => image.ListingId, listing => listing.Id,
                (image, _) => new { image.ObjectKey, image.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        if (metadata is null) return Results.NotFound();
        return await CatalogImages.StreamAsync(http, storage, imageId, metadata.ObjectKey,
            metadata.ContentType, isPublic: false, cancellationToken);
    }

    private static async Task<(Guid OwnerId, IResult? Denied)> AuthorizeReadAsync(HttpContext http,
        IVerifiedListingOwner listingOwner, CancellationToken cancellationToken)
    {
        if (!http.Request.IsHttps) return (Guid.Empty, Error(403, "https_required"));
        var access = await listingOwner.FindListingOwnerAccessAsync(http, cancellationToken);
        if (access is null) return (Guid.Empty, Error(401, "owner_sign_in_required"));
        if (!access.HasOwnerProfile) return (Guid.Empty, Error(403, "owner_account_required"));
        return (access.UserId, null);
    }

    private static async Task<(Guid OwnerId, IResult? Denied)> AuthorizeWriteAsync(HttpContext http,
        IAntiforgery antiforgery, IVerifiedListingOwner listingOwner, CancellationToken cancellationToken)
    {
        if (!http.Request.IsHttps) return (Guid.Empty, Error(403, "https_required"));
        try { await antiforgery.ValidateRequestAsync(http); }
        catch (AntiforgeryValidationException) { return (Guid.Empty, Error(400, "invalid_csrf")); }
        var access = await listingOwner.FindListingOwnerAccessAsync(http, cancellationToken);
        if (access is null) return (Guid.Empty, Error(401, "owner_sign_in_required"));
        if (!access.HasOwnerProfile) return (Guid.Empty, Error(403, "owner_account_required"));
        if (!access.IsApproved) return (Guid.Empty, Error(403, "owner_approval_required"));
        return (access.UserId, null);
    }

    private static IQueryable<OwnerListingRow> OwnerQuery(CatalogDbContext db, Guid ownerId, string? locale) =>
        db.Listings.AsNoTracking().Where(listing => listing.OwnerUserId == ownerId)
            .Join(db.Models.AsNoTracking(), listing => listing.ModelId, model => model.Id,
                (listing, model) => new { listing, model })
            // Member initialization (not a constructor) so EF can translate later Where/OrderBy calls.
            .Join(db.Makes.AsNoTracking(), row => row.model.MakeId, make => make.Id,
                (row, make) => new OwnerListingRow
                {
                    Id = row.listing.Id, MakeSlug = make.Slug,
                    Make = locale == "en" ? make.NameEn : make.NameAz,
                    ModelId = row.model.Id,
                    Model = locale == "en" ? row.model.NameEn : row.model.NameAz,
                    Year = row.listing.Year, City = row.listing.City, DailyPrice = row.listing.DailyPrice,
                    Transmission = row.listing.Transmission, FuelType = row.listing.FuelType, Seats = row.listing.Seats,
                    DescriptionAz = row.listing.DescriptionAz, DescriptionEn = row.listing.DescriptionEn,
                    Status = row.listing.Status, CreatedAtUtc = row.listing.CreatedAtUtc,
                    UpdatedAtUtc = row.listing.UpdatedAtUtc
                });

    private static async Task<List<object>> DecorateAsync(CatalogDbContext db, List<OwnerListingRow> rows,
        CancellationToken cancellationToken)
    {
        var ids = rows.Select(row => row.Id).ToArray();
        var images = await db.ListingImages.AsNoTracking().Where(image => ids.Contains(image.ListingId))
            .OrderBy(image => image.Position).Select(image => new { image.ListingId, image.Id })
            .ToListAsync(cancellationToken);
        var rejections = (await db.ListingReviewEvents.AsNoTracking()
                .Where(review => ids.Contains(review.ListingId) && review.Decision == "Rejected")
                .OrderByDescending(review => review.CreatedAtUtc)
                .Select(review => new { review.ListingId, review.ReasonCode })
                .ToListAsync(cancellationToken))
            .DistinctBy(review => review.ListingId) // latest rejection per listing
            .ToDictionary(review => review.ListingId, review => review.ReasonCode);
        var pendingBookings = await db.Bookings.AsNoTracking()
            .Where(booking => ids.Contains(booking.ListingId) && booking.Status == BookingStatus.Requested)
            .GroupBy(booking => booking.ListingId)
            .Select(group => new { ListingId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.ListingId, item => item.Count, cancellationToken);
        return rows.Select(row => (object)new
        {
            row.Id, row.MakeSlug, row.Make, row.ModelId, row.Model, row.Year, row.City, row.DailyPrice,
            row.Transmission, row.FuelType, row.Seats, row.DescriptionAz, row.DescriptionEn, row.Status,
            row.CreatedAtUtc, row.UpdatedAtUtc,
            RejectionReason = row.Status == ListingStatus.Rejected ? rejections.GetValueOrDefault(row.Id) : null,
            PendingBookings = pendingBookings.GetValueOrDefault(row.Id),
            Images = images.Where(image => image.ListingId == row.Id)
                .Select(image => CatalogImages.OwnerUrl(row.Id, image.Id)).ToArray()
        }).ToList();
    }

    private static ListingDraft? ParseForm(IFormCollection form)
    {
        static int? Int(string? value) =>
            int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : null;
        decimal? price = decimal.TryParse(form["dailyPrice"], NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        return ListingRules.Validate(Int(form["modelId"]), Int(form["year"]), form["city"].ToString(), price,
            form["transmission"].ToString(), form["fuelType"].ToString(), Int(form["seats"]),
            form["descriptionAz"].ToString(), form["descriptionEn"].ToString());
    }

    private static async Task DeleteObjectsAsync(IVehicleImageStorage storage, IEnumerable<string> keys,
        Guid listingId, ILogger logger)
    {
        foreach (var key in keys)
        {
            try { await storage.DeleteAsync(key, CancellationToken.None); }
            catch (Exception exception) when (exception is AmazonS3Exception or HttpRequestException)
            {
                logger.LogError("Could not delete a stored vehicle image. ListingId: {ListingId}", listingId);
            }
        }
    }

    private sealed record CleanImage(byte[] Bytes, string ContentType);

    private sealed class OwnerListingRow
    {
        public Guid Id { get; init; }
        public string MakeSlug { get; init; } = "";
        public string Make { get; init; } = "";
        public int ModelId { get; init; }
        public string Model { get; init; } = "";
        public short Year { get; init; }
        public string City { get; init; } = "";
        public decimal DailyPrice { get; init; }
        public string Transmission { get; init; } = "";
        public string FuelType { get; init; } = "";
        public byte Seats { get; init; }
        public string DescriptionAz { get; init; } = "";
        public string DescriptionEn { get; init; } = "";
        public string Status { get; init; } = "";
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset? UpdatedAtUtc { get; init; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record UpdateListingRequest(
        int? ModelId, int? Year, string? City, decimal? DailyPrice, string? Transmission,
        string? FuelType, int? Seats, string? DescriptionAz, string? DescriptionEn);
}
