namespace TurboRent.Modules.Catalog.Features.BrowseListings;

public sealed record ListingSearch(
    string? City, string? Make, string? Model, int? MinYear, int? MaxYear,
    decimal? MinPrice, decimal? MaxPrice, string? Transmission, string? FuelType,
    int? Seats, int Page = 1, int PageSize = 12, string? Sort = null);

public sealed record ListingSummary(
    Guid Id, string Make, string Model, short Year, string City, decimal DailyPrice,
    string Transmission, string FuelType, byte Seats, DateTimeOffset CreatedAtUtc, string[] Images);

public sealed record ListingDetails(
    Guid Id, string Make, string Model, short Year, string City, decimal DailyPrice,
    string Transmission, string FuelType, byte Seats, string Description, DateTimeOffset CreatedAtUtc,
    string? OwnerName, bool OwnerIsCompany, string[] Images);
