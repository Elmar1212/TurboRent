namespace TurboRent.Modules.Catalog.Domain;

public sealed class VehicleListingImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public string ObjectKey { get; set; } = "";
    public string ContentType { get; set; } = "";
    public int Length { get; set; }
    public short Position { get; set; }
}
