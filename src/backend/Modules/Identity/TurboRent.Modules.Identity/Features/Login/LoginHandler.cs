using Microsoft.AspNetCore.Identity;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;
using TurboRent.Modules.Identity.Infrastructure.Security;

namespace TurboRent.Modules.Identity.Features.Login;

public sealed class LoginHandler(
    IdentityDbContext db,
    UserManager<ApplicationUser> users,
    AdminTotpService totp)
{
    private static readonly string DummyPasswordHash =
        new PasswordHasher<ApplicationUser>().HashPassword(new ApplicationUser(), "TurboRent invalid login timing passphrase");

    public async Task<LoginResult> Handle(LoginRequest request, string traceId, CancellationToken ct)
    {
        if (request.Email is not { Length: > 0 and <= 254 } email ||
            request.Password is not { Length: > 0 and <= 128 } password)
            return LoginResult.Failed("invalid_credentials");

        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            _ = users.PasswordHasher.VerifyHashedPassword(new ApplicationUser(), DummyPasswordHash, password);
            await Audit(null, "failed", traceId, ct);
            return LoginResult.Failed("invalid_credentials");
        }

        var validPassword = await users.CheckPasswordAsync(user, password);
        if (!validPassword)
        {
            if (!await users.IsLockedOutAsync(user))
            {
                var failed = await users.AccessFailedAsync(user);
                if (!failed.Succeeded) throw new InvalidOperationException("Failed login attempt could not be recorded.");
            }
            await Audit(user.Id, "failed", traceId, ct);
            return LoginResult.Failed("invalid_credentials");
        }

        if (await users.IsLockedOutAsync(user) || !user.EmailConfirmed || user.AccountStatus != "Active")
        {
            await Audit(user.Id, "rejected", traceId, ct);
            return LoginResult.Failed("invalid_credentials");
        }

        var isAdmin = await users.IsInRoleAsync(user, IdentityRoles.Admin);
        if (isAdmin && string.IsNullOrEmpty(request.TotpCode))
        {
            await Audit(user.Id, "mfa_required", traceId, ct);
            return LoginResult.Failed("mfa_required");
        }
        if (isAdmin && !await totp.VerifyAndConsumeAsync(user, request.TotpCode))
        {
            if (!await users.IsLockedOutAsync(user))
            {
                var failed = await users.AccessFailedAsync(user);
                if (!failed.Succeeded) throw new InvalidOperationException("Failed MFA attempt could not be recorded.");
            }
            await Audit(user.Id, "mfa_failed", traceId, ct);
            return LoginResult.Failed("mfa_invalid");
        }

        var reset = await users.ResetAccessFailedCountAsync(user);
        if (!reset.Succeeded) throw new InvalidOperationException("Login lockout state could not be reset.");
        var now = DateTimeOffset.UtcNow;
        var session = ApplicationSession.Create(user.Id, admin: isAdmin, mfaVerified: isAdmin, now);
        db.Sessions.Add(session);
        db.SecurityAuditRecords.Add(new SecurityAuditRecord
        {
            ActorUserId = user.Id, SubjectUserId = user.Id, EventCode = "login",
            ResultCode = "succeeded", TraceId = traceId, OccurredAtUtc = now
        });
        await db.SaveChangesAsync(ct);
        return LoginResult.Succeeded(user, session);
    }

    private async Task Audit(Guid? userId, string result, string traceId, CancellationToken ct)
    {
        db.SecurityAuditRecords.Add(new SecurityAuditRecord
        {
            ActorUserId = userId, SubjectUserId = userId, EventCode = "login",
            ResultCode = result, TraceId = traceId, OccurredAtUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}

public sealed record LoginResult(string? Code, ApplicationUser? User, ApplicationSession? Session)
{
    public static LoginResult Failed(string code) => new(code, null, null);
    public static LoginResult Succeeded(ApplicationUser user, ApplicationSession session) => new(null, user, session);
    public bool IsSuccess => User is not null && Session is not null;
}
