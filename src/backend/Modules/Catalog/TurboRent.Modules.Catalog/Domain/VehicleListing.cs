namespace TurboRent.Modules.Catalog.Domain;

public sealed class VehicleListing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? OwnerUserId { get; set; }
    public int ModelId { get; set; }
    public short Year { get; set; }
    public string City { get; set; } = "";
    public decimal DailyPrice { get; set; }
    public string Transmission { get; set; } = "";
    public string FuelType { get; set; } = "";
    public byte Seats { get; set; }
    public string DescriptionAz { get; set; } = "";
    public string DescriptionEn { get; set; } = "";
    public string Status { get; set; } = ListingStatus.Draft;
    public DateTimeOffset CreatedAtUtc { get; set; }
    /// <summary>Last owner edit; null when the listing was never edited.</summary>
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public VehicleModel Model { get; set; } = null!;
    public List<VehicleListingImage> Images { get; set; } = [];
}

/// <summary>Listing lifecycle. Only <see cref="Approved"/> listings are public.</summary>
public static class ListingStatus
{
    public const string Draft = "Draft";
    /// <summary>Waiting for Admin review, after creation or after an owner edit.</summary>
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Suspended = "Suspended";
    /// <summary>Hidden by the owner; can be restored, which sends it back to review.</summary>
    public const string Archived = "Archived";
}
