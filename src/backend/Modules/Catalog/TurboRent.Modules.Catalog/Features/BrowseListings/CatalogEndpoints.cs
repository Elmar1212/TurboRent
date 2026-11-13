using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TurboRent.BuildingBlocks.Http;
using TurboRent.Modules.Catalog.Domain;
using TurboRent.Modules.Catalog.Features.Shared;
using TurboRent.Modules.Catalog.Infrastructure.Persistence;
using TurboRent.Modules.Catalog.Infrastructure.Storage;
using TurboRent.Modules.Identity.Contracts;
using static TurboRent.BuildingBlocks.Http.ApiProblem;

namespace TurboRent.Modules.Catalog.Features.BrowseListings;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalog(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/catalog").WithTags("Vehicle catalog");
        group.MapGet("/makes", async (string? locale, HttpContext http, CatalogDbContext db, CancellationToken ct) =>
        {
            if (locale is not (null or "az" or "en")) return InvalidQuery();
            var makes = await db.Makes.AsNoTracking().OrderBy(make => make.NameEn)
                .Select(make => new
                {
                    make.Slug,
                    Name = locale == "en" ? make.NameEn : make.NameAz,
                    Models = make.Models.OrderBy(model => model.NameEn).Select(model => new
                    {
                        model.Id,
                        model.Slug,
                        Name = locale == "en" ? model.NameEn : model.NameAz
                    })
                }).ToListAsync(ct);
            // Reference data changes only with a migration.
            http.Response.Headers.CacheControl = "public, max-age=3600";
            return Results.Ok(makes);
        });

        group.MapGet("/listings", async (
            string? locale, string? city, string? make, string? model, int? minYear, int? maxYear,
            decimal? minPrice, decimal? maxPrice, string? transmission, string? fuelType, int? seats,
            int? page, int? pageSize, string? sort, CatalogDbContext db, CancellationToken ct) =>
        {
            var search = new ListingSearch(city, make, model, minYear, maxYear, minPrice, maxPrice,
                transmission, fuelType, seats, page ?? 1, pageSize ?? 12, sort);
            if (locale is not (null or "az" or "en") || !ListingSearchValidator.IsValid(search))
                return InvalidQuery();

            var query = db.Listings.AsNoTracking().Where(listing => listing.Status == ListingStatus.Approved)
                .Join(db.Models.AsNoTracking(), listing => listing.ModelId, vehicleModel => vehicleModel.Id,
                    (listing, vehicleModel) => new { listing, vehicleModel })
                .Join(db.Makes.AsNoTracking(), row => row.vehicleModel.MakeId, vehicleMake => vehicleMake.Id,
                    (row, vehicleMake) => new { row.listing, row.vehicleModel, vehicleMake });
            if (city is not null) query = query.Where(row => row.listing.City == city);
            if (make is not null) query = query.Where(row => row.vehicleMake.Slug == make);
            if (model is not null) query = query.Where(row => row.vehicleModel.Slug == model);
            if (minYear is not null) query = query.Where(row => row.listing.Year >= minYear);
            if (maxYear is not null) query = query.Where(row => row.listing.Year <= maxYear);
            if (minPrice is not null) query = query.Where(row => row.listing.DailyPrice >= minPrice);
            if (maxPrice is not null) query = query.Where(row => row.listing.DailyPrice <= maxPrice);
            if (transmission is not null) query = query.Where(row => row.listing.Transmission == transmission);
            if (fuelType is not null) query = query.Where(row => row.listing.FuelType == fuelType);
            if (seats is not null) query = query.Where(row => row.listing.Seats >= seats);

            var total = await query.CountAsync(ct);
            var ordered = (sort ?? "newest") switch
            {
                "price_asc" => query.OrderBy(row => row.listing.DailyPrice).ThenBy(row => row.listing.Id),
                "price_desc" => query.OrderByDescending(row => row.listing.DailyPrice).ThenBy(row => row.listing.Id),
                "year_desc" => query.OrderByDescending(row => row.listing.Year).ThenBy(row => row.listing.Id),
                _ => query.OrderByDescending(row => row.listing.CreatedAtUtc).ThenBy(row => row.listing.Id)
            };
            var items = await ordered
                .Skip((search.Page - 1) * search.PageSize).Take(search.PageSize)
                .Select(row => new ListingSummary(row.listing.Id,
                    locale == "en" ? row.vehicleMake.NameEn : row.vehicleMake.NameAz,
                    locale == "en" ? row.vehicleModel.NameEn : row.vehicleModel.NameAz,
                    row.listing.Year, row.listing.City, row.listing.DailyPrice,
                    row.listing.Transmission, row.listing.FuelType, row.listing.Seats,
                    row.listing.CreatedAtUtc, Array.Empty<string>()))
                .ToListAsync(ct);
            var ids = items.Select(item => item.Id).ToArray();
            var images = await db.ListingImages.AsNoTracking().Where(image => ids.Contains(image.ListingId))
                .OrderBy(image => image.Position)
                .Select(image => new { image.ListingId, image.Id })
                .ToListAsync(ct);
            items = items.Select(item => item with
            {
                Images = images.Where(image => image.ListingId == item.Id)
                    .Select(image => CatalogImages.PublicUrl(item.Id, image.Id)).ToArray()
            }).ToList();
            return Results.Ok(new { items, total, page = search.Page, pageSize = search.PageSize });
        });

        group.MapGet("/listings/{id:guid}", async (Guid id, string? locale, CatalogDbContext db,
            IListingOwnerDirectory owners, CancellationToken ct) =>
        {
            if (locale is not (null or "az" or "en")) return InvalidQuery();
            var row = await db.Listings.AsNoTracking()
                .Where(listing => listing.Id == id && listing.Status == ListingStatus.Approved)
                .Join(db.Models.AsNoTracking(), listing => listing.ModelId, vehicleModel => vehicleModel.Id,
                    (listing, vehicleModel) => new { listing, vehicleModel })
                .Join(db.Makes.AsNoTracking(), row => row.vehicleModel.MakeId, vehicleMake => vehicleMake.Id,
                    (row, vehicleMake) => new
                    {
                        row.listing.OwnerUserId,
                        Details = new ListingDetails(row.listing.Id,
                            locale == "en" ? vehicleMake.NameEn : vehicleMake.NameAz,
                            locale == "en" ? row.vehicleModel.NameEn : row.vehicleModel.NameAz,
                            row.listing.Year, row.listing.City, row.listing.DailyPrice,
                            row.listing.Transmission, row.listing.FuelType, row.listing.Seats,
                            locale == "en" ? row.listing.DescriptionEn : row.listing.DescriptionAz,
                            row.listing.CreatedAtUtc, null, false, Array.Empty<string>())
                    })
                .SingleOrDefaultAsync(ct);
            if (row is null) return Results.NotFound();
            var imageIds = await db.ListingImages.AsNoTracking()
                .Where(image => image.ListingId == id).OrderBy(image => image.Position)
                .Select(image => image.Id).ToArrayAsync(ct);
            ListingOwnerIdentity? owner = null;
            if (row.OwnerUserId is { } ownerId)
                (await owners.FindAsync([ownerId], ct)).TryGetValue(ownerId, out owner);
            return Results.Ok(row.Details with
            {
                // Email addresses are never public; the phone number is revealed on request.
                OwnerName = owner?.CompanyName ?? owner?.DisplayName,
                OwnerIsCompany = owner?.CompanyName is not null,
                Images = imageIds.Select(imageId => CatalogImages.PublicUrl(id, imageId)).ToArray()
            });
        });

        // Separate, rate-limited endpoint so phone numbers cannot be scraped from listing pages.
        group.MapGet("/listings/{id:guid}/contact", async (Guid id, HttpContext http, CatalogDbContext db,
            IListingOwnerDirectory owners, CancellationToken ct) =>
        {
            NoStore(http);
            var ownerId = await db.Listings.AsNoTracking()
                .Where(listing => listing.Id == id && listing.Status == ListingStatus.Approved)
                .Select(listing => listing.OwnerUserId).SingleOrDefaultAsync(ct);
            if (ownerId is null) return Results.NotFound();
            if (!(await owners.FindAsync([ownerId.Value], ct)).TryGetValue(ownerId.Value, out var owner) ||
                owner.ContactPhone is null)
                return Results.NotFound();
            return Results.Ok(new { name = owner.CompanyName ?? owner.DisplayName, phone = owner.ContactPhone });
        }).RequireRateLimiting(RateLimitPolicies.Contact);

        // Days already confirmed for other renters, so the booking form can block them.
        group.MapGet("/listings/{id:guid}/availability", async (Guid id, CatalogDbContext db,
            TimeProvider time, CancellationToken ct) =>
        {
            var exists = await db.Listings.AsNoTracking()
                .AnyAsync(listing => listing.Id == id && listing.Status == ListingStatus.Approved, ct);
            if (!exists) return Results.NotFound();
            var today = BakuCalendar.Today(time);
            var until = today.AddDays(365);
            var booked = await db.Bookings.AsNoTracking()
                .Where(booking => booking.ListingId == id && booking.Status == BookingStatus.Confirmed &&
                    booking.EndDate >= today && booking.StartDate <= until)
                .OrderBy(booking => booking.StartDate)
                .Select(booking => new { startDate = booking.StartDate, endDate = booking.EndDate })
                .ToListAsync(ct);
            return Results.Ok(new { today, booked });
        });

        group.MapGet("/listings/{id:guid}/images/{imageId:guid}", async (
            Guid id, Guid imageId, HttpContext http, CatalogDbContext db, IVehicleImageStorage storage, CancellationToken ct) =>
        {
            var metadata = await db.ListingImages.AsNoTracking()
                .Where(image => image.Id == imageId && image.ListingId == id)
                .Join(db.Listings.AsNoTracking().Where(listing => listing.Status == ListingStatus.Approved),
                    image => image.ListingId, listing => listing.Id,
                    (image, _) => new { image.ObjectKey, image.ContentType })
                .SingleOrDefaultAsync(ct);
            if (metadata is null) return Results.NotFound();
            return await CatalogImages.StreamAsync(http, storage, imageId, metadata.ObjectKey,
                metadata.ContentType, isPublic: true, ct);
        });
        return endpoints;
    }

    private static IResult InvalidQuery() => Error(400, "invalid_catalog_query");
}
