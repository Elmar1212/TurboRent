using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Email;
using TurboRent.Modules.Identity.Infrastructure.Persistence;
using TurboRent.Modules.Identity.Infrastructure.Security;
using TurboRent.Modules.Identity.Validation;
using static TurboRent.BuildingBlocks.Http.ApiProblem;

namespace TurboRent.Modules.Identity.Features.Password;

/// <summary>Forgot-password, reset-password and change-password flows.</summary>
public static class PasswordEndpoints
{
    /// <summary>Maps the endpoints onto the CSRF-protected, rate-limited auth write group.</summary>
    public static void MapPasswordEndpoints(this RouteGroupBuilder writes)
    {
        writes.MapPost("/password/forgot", async (ForgotPasswordRequest request, HttpContext http,
            UserManager<ApplicationUser> users, IdentityLimits limits, IIdentityEmail email,
            IdentityDbContext db, ILoggerFactory loggers) =>
        {
            if (!RegistrationValidation.ValidEmail(request.Email)) return Error(400, "invalid_email");
            using var lease = limits.Acquire("forgot", users.NormalizeEmail(request.Email!)!);
            if (!lease.IsAcquired)
            {
                http.Response.Headers.RetryAfter = RetryAfter.Seconds(lease, TimeSpan.FromMinutes(15));
                return Error(429, "rate_limited");
            }
            var user = await users.FindByEmailAsync(request.Email!);
            // The response is identical whether or not the account exists, to avoid account enumeration.
            if (user is { EmailConfirmed: true, AccountStatus: "Active" })
            {
                var token = await users.GeneratePasswordResetTokenAsync(user);
                try { await email.SendPasswordResetAsync(user, token, http.RequestAborted); }
                catch (Exception exception) when (IdentityEmail.IsDeliveryFailure(exception))
                {
                    loggers.CreateLogger("PasswordReset").LogWarning(
                        "Password reset delivery failed. FailureType: {FailureType}. TraceId: {TraceId}",
                        exception.GetType().Name, http.TraceIdentifier);
                }
                await Audit(db, user.Id, user.Id, "password_reset_requested", "sent", http);
            }
            return Results.Accepted(value: new { code = "password_reset_requested" });
        });

        writes.MapPost("/password/reset", async (ResetPasswordRequest request, HttpContext http,
            UserManager<ApplicationUser> users, IdentityLimits limits, IdentityDbContext db) =>
        {
            if (request.UserId == Guid.Empty || request.Token is not { Length: > 0 and <= 4096 })
                return Error(400, "invalid_reset_link");
            if (request.Password is not { Length: >= 15 and <= 128 })
                return Error(400, "password_length");
            using var lease = limits.Acquire("reset", request.UserId.ToString("N"));
            if (!lease.IsAcquired)
            {
                http.Response.Headers.RetryAfter = RetryAfter.Seconds(lease, TimeSpan.FromMinutes(15));
                return Error(429, "rate_limited");
            }
            string decoded;
            try { decoded = System.Text.Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token)); }
            catch (FormatException) { return Error(400, "invalid_reset_link"); }

            var user = await users.FindByIdAsync(request.UserId.ToString());
            if (user is not { EmailConfirmed: true, AccountStatus: "Active" }) return Error(400, "invalid_reset_link");
            var result = await users.ResetPasswordAsync(user, decoded, request.Password);
            if (!result.Succeeded)
                return result.Errors.Any(error => error.Code == "InvalidToken")
                    ? Error(400, "invalid_reset_link") : Error(400, "password_length");

            // A new password ends every existing session and clears any lockout.
            await users.ResetAccessFailedCountAsync(user);
            await users.SetLockoutEndDateAsync(user, null);
            await RevokeSessions(db, user.Id, keep: null);
            await Audit(db, user.Id, user.Id, "password_reset", "succeeded", http);
            return Results.Ok(new { code = "password_reset" });
        });

        writes.MapPost("/password/change", async (ChangePasswordRequest request, HttpContext http,
            UserManager<ApplicationUser> users, SessionAuthenticator sessions, IdentityDbContext db) =>
        {
            var active = await sessions.AuthenticateAsync(http, http.RequestAborted);
            if (active is null) return Error(401, "not_authenticated");
            if (request.CurrentPassword is not { Length: > 0 and <= 128 }) return Error(400, "invalid_current_password");
            if (request.NewPassword is not { Length: >= 15 and <= 128 }) return Error(400, "password_length");
            var result = await users.ChangePasswordAsync(active.User, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                await Audit(db, active.User.Id, active.User.Id, "password_change", "failed", http);
                return result.Errors.Any(error => error.Code == "PasswordMismatch")
                    ? Error(400, "invalid_current_password") : Error(400, "password_length");
            }
            // Other devices are signed out; this one stays signed in.
            await RevokeSessions(db, active.User.Id, keep: active.Session.Id);
            await Audit(db, active.User.Id, active.User.Id, "password_change", "succeeded", http);
            return Results.Ok(new { code = "password_changed" });
        });
    }

    private static async Task RevokeSessions(IdentityDbContext db, Guid userId, Guid? keep)
    {
        var now = DateTimeOffset.UtcNow;
        var sessions = await db.Sessions
            .Where(session => session.UserId == userId && session.RevokedAtUtc == null && session.Id != keep)
            .ToListAsync();
        foreach (var session in sessions) session.Revoke(now);
        await db.SaveChangesAsync();
    }

    private static async Task Audit(IdentityDbContext db, Guid actor, Guid subject, string eventCode,
        string result, HttpContext http)
    {
        db.SecurityAuditRecords.Add(new SecurityAuditRecord
        {
            ActorUserId = actor, SubjectUserId = subject, EventCode = eventCode,
            ResultCode = result, TraceId = http.TraceIdentifier, OccurredAtUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(http.RequestAborted);
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record ForgotPasswordRequest(string? Email);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record ResetPasswordRequest(Guid UserId, string? Token, string? Password);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);
}

/// <summary>Password reset links expire sooner than email verification links.</summary>
public sealed class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public const string ProviderName = "TurboRentPasswordReset";

    public PasswordResetTokenProviderOptions()
    {
        Name = ProviderName;
        TokenLifespan = TimeSpan.FromHours(2);
    }
}

public sealed class PasswordResetTokenProvider(
    IDataProtectionProvider protection,
    IOptions<PasswordResetTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<ApplicationUser>> logger)
    : DataProtectorTokenProvider<ApplicationUser>(protection, options, logger);
