using TurboRent.Modules.Catalog.Features.BrowseListings;

namespace TurboRent.Identity.UnitTests;

public class ListingSearchValidatorTests
{
    [Fact]
    public void Empty_search_uses_safe_defaults() =>
        Assert.True(ListingSearchValidator.IsValid(new ListingSearch(null, null, null, null, null,
            null, null, null, null, null)));

    [Theory]
    [InlineData("unknown-city", null, null, null, null)]
    [InlineData("baku", null, "camry", null, null)]
    [InlineData("baku", null, null, 2030, 2020)]
    [InlineData("baku", "bad slug", null, null, null)]
    public void Invalid_text_and_year_filter_values_are_rejected(string city, string? make,
        string? model, int? minYear, int? maxYear)
    {
        var search = new ListingSearch(city, make, model, minYear, maxYear, null, null,
            null, null, null);
        Assert.False(ListingSearchValidator.IsValid(search));
    }

    [Fact]
    public void Invalid_price_bounds_are_rejected()
    {
        var valid = new ListingSearch("baku", "toyota", null, null, null, null, null,
            null, null, null);
        Assert.False(ListingSearchValidator.IsValid(valid with { MinPrice = 100m, MaxPrice = 50m }));
        Assert.False(ListingSearchValidator.IsValid(valid with { MinPrice = 0m }));
    }

    [Theory]
    [InlineData("electric", "automatic", 5)]
    [InlineData("diesel", "manual", 7)]
    public void Supported_specs_and_bounds_are_accepted(string fuel, string transmission, int seats) =>
        Assert.True(ListingSearchValidator.IsValid(new ListingSearch("baku", "toyota", "camry",
            2010, 2025, 10, 500, transmission, fuel, seats)));

    [Fact]
    public void Pagination_is_bounded() =>
        Assert.False(ListingSearchValidator.IsValid(new ListingSearch(null, null, null, null, null,
            null, null, null, null, null, 1, 49)));
}
