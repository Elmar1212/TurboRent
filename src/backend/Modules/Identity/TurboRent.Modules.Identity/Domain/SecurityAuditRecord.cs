namespace TurboRent.Modules.Identity.Domain;

// Only allowlisted event/result codes belong here. No request body, token or password fields.
public sealed class SecurityAuditRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ActorUserId { get; set; }
    public Guid? SubjectUserId { get; set; }
    public string EventCode { get; set; } = "";
    public string ResultCode { get; set; } = "";
    public string TraceId { get; set; } = "";
    public DateTimeOffset OccurredAtUtc { get; set; }
}
