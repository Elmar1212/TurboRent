namespace TurboRent.Modules.Identity.Domain;

public sealed class ApplicationSession
{
    private ApplicationSession() { }
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsAdminSession { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset LastSeenAtUtc { get; private set; }
    public DateTimeOffset IdleExpiresAtUtc { get; private set; }
    public DateTimeOffset AbsoluteExpiresAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public DateTimeOffset? MfaVerifiedAtUtc { get; private set; }
    public Guid Version { get; private set; }

    public static ApplicationSession Create(Guid userId, bool admin, bool mfaVerified, DateTimeOffset now)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A user is required.", nameof(userId));
        if (admin && !mfaVerified) throw new InvalidOperationException("Admin sessions require MFA.");
        now = now.ToUniversalTime();
        return new ApplicationSession
        {
            Id = Guid.NewGuid(), UserId = userId, IsAdminSession = admin,
            CreatedAtUtc = now, LastSeenAtUtc = now,
            IdleExpiresAtUtc = now.AddMinutes(admin ? 15 : 30),
            AbsoluteExpiresAtUtc = admin ? now.AddHours(8) : now.AddDays(7),
            MfaVerifiedAtUtc = mfaVerified ? now : null,
            Version = Guid.NewGuid()
        };
    }

    public bool IsActive(DateTimeOffset now) =>
        RevokedAtUtc is null && now < IdleExpiresAtUtc && now < AbsoluteExpiresAtUtc;

    public void Touch(DateTimeOffset now)
    {
        now = now.ToUniversalTime();
        if (!IsActive(now)) throw new InvalidOperationException("Session is inactive.");
        if (now < LastSeenAtUtc) throw new InvalidOperationException("Time cannot move backwards.");
        LastSeenAtUtc = now;
        var idle = now.AddMinutes(IsAdminSession ? 15 : 30);
        IdleExpiresAtUtc = idle < AbsoluteExpiresAtUtc ? idle : AbsoluteExpiresAtUtc;
        Version = Guid.NewGuid();
    }

    public void Revoke(DateTimeOffset now)
    {
        RevokedAtUtc ??= now.ToUniversalTime();
        Version = Guid.NewGuid();
    }
}
