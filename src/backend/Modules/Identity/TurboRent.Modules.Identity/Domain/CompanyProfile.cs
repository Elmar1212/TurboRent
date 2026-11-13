namespace TurboRent.Modules.Identity.Domain;

public sealed class CompanyProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string LegalName { get; set; } = "";
    public string Voen { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public ApprovalStatus Status { get; private set; } = ApprovalStatus.Pending;
    public DateTimeOffset SubmittedAtUtc { get; set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public string? ReasonCode { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();

    public void Review(ApprovalStatus next, Guid reviewer, string? reasonCode, DateTimeOffset now)
    {
        ApprovalRules.ValidateReview(Status, next, UserId, reviewer, reasonCode);
        Status = next;
        ReviewedByUserId = reviewer;
        ReviewedAtUtc = now.ToUniversalTime();
        ReasonCode = reasonCode;
        Version = Guid.NewGuid();
    }
}
