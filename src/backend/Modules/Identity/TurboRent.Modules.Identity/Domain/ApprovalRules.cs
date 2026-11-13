namespace TurboRent.Modules.Identity.Domain;

public enum ApprovalStatus { Pending, Approved, Rejected, Suspended }

public static class ApprovalRules
{
    // Authorization of the reviewer is also required at the application boundary.
    public static void ValidateReview(
        ApprovalStatus current, ApprovalStatus next, Guid applicant, Guid reviewer, string? reasonCode)
    {
        if (applicant == reviewer)
            throw new InvalidOperationException("Self-review is not permitted.");
        var allowed = (current, next) switch
        {
            (ApprovalStatus.Pending, ApprovalStatus.Approved or ApprovalStatus.Rejected) => true,
            (ApprovalStatus.Approved, ApprovalStatus.Suspended) => true,
            (ApprovalStatus.Suspended, ApprovalStatus.Approved) => true,
            _ => false
        };
        if (!allowed)
            throw new InvalidOperationException("Invalid approval transition.");
        if (next is ApprovalStatus.Rejected or ApprovalStatus.Suspended &&
            string.IsNullOrWhiteSpace(reasonCode))
            throw new ArgumentException("A reason code is required.", nameof(reasonCode));
    }
}
