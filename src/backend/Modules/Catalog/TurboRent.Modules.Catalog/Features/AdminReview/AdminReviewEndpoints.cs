using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TurboRent.Modules.Catalog.Domain;
using TurboRent.Modules.Catalog.Infrastructure.Persistence;
using TurboRent.Modules.Catalog.Infrastructure.Storage;
using TurboRent.Modules.Identity.Contracts;
using TurboRent.BuildingBlocks.Http;
using TurboRent.Modules.Catalog.Features.Shared;
using static TurboRent.BuildingBlocks.Http.ApiProblem;

namespace TurboRent.Modules.Catalog.Features.AdminReview;

public static class AdminReviewEndpoints
{
    private static readonly HashSet<string> RejectionReasons =
        ["missing_information", "invalid_vehicle_details", "image_quality", "other"];

    public static IEndpointRouteBuilder MapAdminReview(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/listings")
            .WithTags("Admin listing review")
            .RequireRateLimiting(RateLimitPolicies.Dashboard);

        group.MapGet("", async (
            string? locale,
            int? page,
            int? pageSize,
            HttpContext http,
            IVerifiedAdmin verifiedAdmin,
            IListingOwnerDirectory ownerDirectory,
            CatalogDbContext db,
            CancellationToken cancellationToken) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            if (await verifiedAdmin.FindVerifiedAdminAsync(http, cancellationToken) is null)
                return Error(401, "admin_sign_in_required");
            var currentPage = page ?? 1;
            var currentPageSize = pageSize ?? 20;
            if (currentPage is < 1 or > 10000 || currentPageSize is < 1 or > 50)
                return Error(400, "invalid_admin_query");

            var pending = db.Listings.AsNoTracking().Where(listing => listing.Status == "Pending");
            var total = await pending.CountAsync(cancellationToken);
            var items = await pending
                .Join(db.Models.AsNoTracking(), listing => listing.ModelId, model => model.Id,
                    (listing, model) => new { listing, model })
                .Join(db.Makes.AsNoTracking(), row => row.model.MakeId, make => make.Id,
                    (row, make) => new
                    {
                        row.listing.Id,
                        Make = locale == "en" ? make.NameEn : make.NameAz,
                        Model = locale == "en" ? row.model.NameEn : row.model.NameAz,
                        row.listing.Year,
                        row.listing.City,
                        row.listing.DailyPrice,
                        row.listing.Transmission,
                        row.listing.FuelType,
                        row.listing.Seats,
                        row.listing.DescriptionAz,
                        row.listing.DescriptionEn,
                        row.listing.OwnerUserId,
                        row.listing.CreatedAtUtc
                    })
                .OrderBy(item => item.CreatedAtUtc).ThenBy(item => item.Id)
                .Skip((currentPage - 1) * currentPageSize).Take(currentPageSize)
                .ToListAsync(cancellationToken);
            var ids = items.Select(item => item.Id).ToArray();
            var ownerIds = items.Where(item => item.OwnerUserId.HasValue)
                .Select(item => item.OwnerUserId!.Value).Distinct().ToArray();
            var owners = await ownerDirectory.FindAsync(ownerIds, cancellationToken);
            var images = await db.ListingImages.AsNoTracking()
                .Where(image => ids.Contains(image.ListingId))
                .OrderBy(image => image.Position)
                .Select(image => new { image.ListingId, image.Id })
                .ToListAsync(cancellationToken);
            return Results.Ok(new
            {
                items = items.Select(item => new
                {
                    item.Id,
                    item.Make,
                    item.Model,
                    item.Year,
                    item.City,
                    item.DailyPrice,
                    item.Transmission,
                    item.FuelType,
                    item.Seats,
                    item.DescriptionAz,
                    item.DescriptionEn,
                    OwnerName = item.OwnerUserId is { } ownerId && owners.TryGetValue(ownerId, out var owner)
                        ? owner.DisplayName : null,
                    OwnerEmail = item.OwnerUserId is { } emailOwnerId && owners.TryGetValue(emailOwnerId, out var emailOwner)
                        ? emailOwner.Email : null,
                    item.CreatedAtUtc,
                    Images = images.Where(image => image.ListingId == item.Id)
                        .Select(image => CatalogImages.AdminUrl(item.Id, image.Id)).ToArray()
                }),
                total,
                page = currentPage,
                pageSize = currentPageSize
            });
        });

        group.MapGet("/{id:guid}/images/{imageId:guid}", async (
            Guid id,
            Guid imageId,
            HttpContext http,
            IVerifiedAdmin verifiedAdmin,
            CatalogDbContext db,
            IVehicleImageStorage storage,
            CancellationToken cancellationToken) =>
        {
            if (await verifiedAdmin.FindVerifiedAdminAsync(http, cancellationToken) is null)
                return Error(401, "admin_sign_in_required");
            var metadata = await db.ListingImages.AsNoTracking()
                .Where(image => image.Id == imageId && image.ListingId == id)
                .Join(db.Listings.AsNoTracking().Where(listing => listing.Status == "Pending"),
                    image => image.ListingId, listing => listing.Id,
                    (image, _) => new { image.ObjectKey, image.ContentType })
                .SingleOrDefaultAsync(cancellationToken);
            if (metadata is null) return Results.NotFound();
            return await CatalogImages.StreamAsync(http, storage, imageId, metadata.ObjectKey,
                metadata.ContentType, isPublic: false, cancellationToken);
        });

        group.MapPost("/{id:guid}/approve", async (
            Guid id,
            HttpContext http,
            IAntiforgery antiforgery,
            IVerifiedAdmin verifiedAdmin,
            CatalogDbContext db,
            IUserNotifier notifier,
            CancellationToken cancellationToken) =>
        {
            var adminId = await ValidateWriteAsync(http, antiforgery, verifiedAdmin, cancellationToken);
            if (adminId is null) return Error(http.Request.IsHttps ? 401 : 403,
                http.Request.IsHttps ? "admin_sign_in_required" : "https_required");
            var listing = await db.Listings.SingleOrDefaultAsync(
                item => item.Id == id && item.Status == ListingStatus.Pending, cancellationToken);
            if (listing is null) return Results.NotFound();
            listing.Status = ListingStatus.Approved;
            db.ListingReviewEvents.Add(new ListingReviewEvent
            {
                ListingId = listing.Id,
                AdminUserId = adminId.Value,
                Decision = "Approved",
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
            try { await db.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateConcurrencyException) { return Error(409, "listing_already_reviewed"); }
            await NotifyOwnerAsync(db, notifier, listing, UserNotificationKind.ListingApproved,
                $"/cars/{listing.Id}", cancellationToken);
            return Results.Ok(new { id = listing.Id, status = listing.Status });
        });

        group.MapPost("/{id:guid}/reject", async (
            Guid id,
            RejectListingRequest request,
            HttpContext http,
            IAntiforgery antiforgery,
            IVerifiedAdmin verifiedAdmin,
            CatalogDbContext db,
            IUserNotifier notifier,
            CancellationToken cancellationToken) =>
        {
            var adminId = await ValidateWriteAsync(http, antiforgery, verifiedAdmin, cancellationToken);
            if (adminId is null) return Error(http.Request.IsHttps ? 401 : 403,
                http.Request.IsHttps ? "admin_sign_in_required" : "https_required");
            if (request.ReasonCode is null || !RejectionReasons.Contains(request.ReasonCode))
                return Error(400, "invalid_rejection_reason");
            var listing = await db.Listings.SingleOrDefaultAsync(
                item => item.Id == id && item.Status == ListingStatus.Pending, cancellationToken);
            if (listing is null) return Results.NotFound();
            listing.Status = ListingStatus.Rejected;
            db.ListingReviewEvents.Add(new ListingReviewEvent
            {
                ListingId = listing.Id,
                AdminUserId = adminId.Value,
                Decision = "Rejected",
                ReasonCode = request.ReasonCode,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
            try { await db.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateConcurrencyException) { return Error(409, "listing_already_reviewed"); }
            await NotifyOwnerAsync(db, notifier, listing, UserNotificationKind.ListingRejected,
                "/owner/listings", cancellationToken);
            return Results.Ok(new { id = listing.Id, status = listing.Status });
        });
        return endpoints;
    }

    private static async Task NotifyOwnerAsync(CatalogDbContext db, IUserNotifier notifier, VehicleListing listing,
        UserNotificationKind kind, string linkPath, CancellationToken cancellationToken)
    {
        if (listing.OwnerUserId is not { } ownerId) return;
        var vehicle = await db.Models.AsNoTracking().Where(model => model.Id == listing.ModelId)
            .Join(db.Makes.AsNoTracking(), model => model.MakeId, make => make.Id,
                (model, make) => make.NameEn + " " + model.NameEn)
            .SingleAsync(cancellationToken);
        await notifier.NotifyAsync(ownerId,
            new UserNotification(kind, $"{listing.Year} {vehicle}", null, linkPath), cancellationToken);
    }

    private static async Task<Guid?> ValidateWriteAsync(
        HttpContext http,
        IAntiforgery antiforgery,
        IVerifiedAdmin verifiedAdmin,
        CancellationToken cancellationToken)
    {
        http.Response.Headers.CacheControl = "no-store";
        http.Response.Headers["Referrer-Policy"] = "no-referrer";
        if (!http.Request.IsHttps) return null;
        try { await antiforgery.ValidateRequestAsync(http); }
        catch (AntiforgeryValidationException) { return null; }
        return await verifiedAdmin.FindVerifiedAdminAsync(http, cancellationToken);
    }


    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record RejectListingRequest(string? ReasonCode);
}
