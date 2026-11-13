namespace TurboRent.Modules.Catalog.Features.BrowseListings;

public static class ListingSearchValidator
{
    public static bool IsValid(ListingSearch search) =>
        (search.City is null || Cities.Contains(search.City)) &&
        (search.Make is null || IsSlug(search.Make, 40)) &&
        (search.Model is null || search.Make is not null && IsSlug(search.Model, 40)) &&
        (search.MinYear is null || search.MinYear is >= 1980 and <= 2035) &&
        (search.MaxYear is null || search.MaxYear is >= 1980 and <= 2035) &&
        (search.MinYear is null || search.MaxYear is null || search.MinYear <= search.MaxYear) &&
        (search.MinPrice is null || search.MinPrice is > 0 and <= 99999) &&
        (search.MaxPrice is null || search.MaxPrice is > 0 and <= 99999) &&
        (search.MinPrice is null || search.MaxPrice is null || search.MinPrice <= search.MaxPrice) &&
        (search.Transmission is null || Transmissions.Contains(search.Transmission)) &&
        (search.FuelType is null || FuelTypes.Contains(search.FuelType)) &&
        (search.Seats is null || search.Seats is >= 2 and <= 9) &&
        search.Page is >= 1 and <= 10000 &&
        search.PageSize is >= 1 and <= 48 &&
        (search.Sort is null || Sorts.Contains(search.Sort));

    public static readonly string[] Sorts = ["newest", "price_asc", "price_desc", "year_desc"];
    public static readonly string[] Cities = ["baku", "ganja", "sumgait", "lankaran", "shaki", "gabala"];
    public static readonly string[] Transmissions = ["automatic", "manual"];
    public static readonly string[] FuelTypes = ["petrol", "diesel", "hybrid", "electric"];

    private static bool IsSlug(string value, int maxLength) =>
        value.Length <= maxLength && value.Length > 0 &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character == '-');
}
