namespace TurboRent.Modules.Catalog.Domain;

public sealed class VehicleModel
{
    public int Id { get; set; }
    public int MakeId { get; set; }
    public string Slug { get; set; } = "";
    public string NameAz { get; set; } = "";
    public string NameEn { get; set; } = "";
}
