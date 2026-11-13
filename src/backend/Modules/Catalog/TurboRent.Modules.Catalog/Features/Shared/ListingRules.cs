using TurboRent.Modules.Catalog.Domain;
using TurboRent.Modules.Catalog.Features.BrowseListings;

namespace TurboRent.Modules.Catalog.Features.Shared;

public sealed record ListingDraft(
    int ModelId, int Year, string City, decimal DailyPrice, string Transmission,
    string FuelType, int Seats, string DescriptionAz, string DescriptionEn);

/// <summary>Validation shared by listing creation (multipart form) and editing (JSON).</summary>
public static class ListingRules
{
    public const int MaxImages = 5;
    public const long MaxImageBytes = 10L * 1024 * 1024;
    public const long MaxTotalImageBytes = 50L * 1024 * 1024;
    public const int MinYear = 1980;
    public const int MaxYear = 2035;

    public static ListingDraft? Validate(int? modelId, int? year, string? city, decimal? price,
        string? transmission, string? fuelType, int? seats, string? descriptionAz, string? descriptionEn)
    {
        var az = descriptionAz?.Trim() ?? "";
        var en = descriptionEn?.Trim() ?? "";
        if (modelId is not >= 1 || year is not (>= MinYear and <= MaxYear) ||
            price is not (> 0 and <= 99999) || decimal.Round(price.Value, 2) != price ||
            seats is not (>= 2 and <= 9) ||
            city is null || !ListingSearchValidator.Cities.Contains(city) ||
            transmission is null || !ListingSearchValidator.Transmissions.Contains(transmission) ||
            fuelType is null || !ListingSearchValidator.FuelTypes.Contains(fuelType) ||
            az.Length is < 1 or > 2000 || en.Length > 2000)
            return null;
        return new ListingDraft(modelId.Value, year.Value, city, price.Value, transmission, fuelType, seats.Value,
            az, en.Length == 0 ? az : en);
    }

    public static void Apply(VehicleListing listing, ListingDraft draft)
    {
        listing.ModelId = draft.ModelId;
        listing.Year = (short)draft.Year;
        listing.City = draft.City;
        listing.DailyPrice = draft.DailyPrice;
        listing.Transmission = draft.Transmission;
        listing.FuelType = draft.FuelType;
        listing.Seats = (byte)draft.Seats;
        listing.DescriptionAz = draft.DescriptionAz;
        listing.DescriptionEn = draft.DescriptionEn;
    }
}

/// <summary>Calendar dates for bookings follow the Asia/Baku time zone (UTC+4, no daylight saving).</summary>
public static class BakuCalendar
{
    private static readonly TimeZoneInfo Zone = Resolve();

    public static DateOnly Today(TimeProvider time) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(time.GetUtcNow(), Zone).DateTime);

    private static TimeZoneInfo Resolve()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"); }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("Asia/Baku", TimeSpan.FromHours(4), "Baku", "Baku");
        }
    }
}
