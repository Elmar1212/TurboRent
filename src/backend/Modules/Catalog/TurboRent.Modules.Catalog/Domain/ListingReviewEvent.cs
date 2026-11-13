namespace TurboRent.Modules.Catalog.Domain;

public sealed class ListingReviewEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public Guid AdminUserId { get; set; }
    public string Decision { get; set; } = "";
    public string? ReasonCode { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
