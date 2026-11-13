namespace TurboRent.Modules.Catalog.Domain;

public sealed class VehicleMake
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string NameAz { get; set; } = "";
    public string NameEn { get; set; } = "";
    public List<VehicleModel> Models { get; set; } = [];
}
