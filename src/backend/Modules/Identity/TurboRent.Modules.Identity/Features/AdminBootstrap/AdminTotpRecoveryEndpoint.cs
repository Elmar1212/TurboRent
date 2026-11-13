using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;
using TurboRent.Modules.Identity.Infrastructure.Security;
using TurboRent.Modules.Identity.Validation;
using TurboRent.BuildingBlocks.Http;
using TurboRent.BuildingBlocks.Security;
using static TurboRent.BuildingBlocks.Http.ApiProblem;

namespace TurboRent.Modules.Identity.Features.AdminBootstrap;

public static class AdminTotpRecoveryEndpoint
{
    public static IEndpointRouteBuilder MapLocalAdminTotpRecovery(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/internal/admin/mfa/reset", async (
            TotpResetRequest request,
            HttpContext http,
            IConfiguration configuration,
            IdentityDbContext db,
            UserManager<ApplicationUser> users,
            AdminTotpService totp,
            CancellationToken cancellationToken) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            var requestSize = http.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
            if (requestSize is { IsReadOnly: false }) requestSize.MaxRequestBodySize = 16384;
            if (http.Request.ContentLength is > 16384) return Error(413, "request_too_large");
            if (!http.Request.IsHttps || http.Connection.RemoteIpAddress is not { } address ||
                !IPAddress.IsLoopback(address))
                return Results.NotFound();

            var expectedSecret = configuration["AdminBootstrap:Secret"];
            if (string.IsNullOrEmpty(expectedSecret) ||
                !SecretComparer.FixedTimeEquals(request.BootstrapSecret, expectedSecret))
                return Results.NotFound();
            if (!RegistrationValidation.ValidEmail(request.Email))
                return Error(400, "invalid_admin_email");

            await using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var adminUsers = await db.UserRoles
                .Join(db.Roles.Where(role => role.Name == IdentityRoles.Admin),
                    assignment => assignment.RoleId, role => role.Id, (assignment, _) => assignment.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
            if (adminUsers.Count != 1) return Error(409, "admin_account_count_invalid");

            var user = await users.FindByEmailAsync(request.Email!);
            if (user is null || user.Id != adminUsers[0] ||
                !user.EmailConfirmed || user.AccountStatus != "Active")
                return Results.NotFound();

            var secret = AdminTotpService.GenerateSecret();
            var result = await totp.SetSecretAsync(user, secret);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Error(500, "admin_mfa_reset_failed");
            }
            user.LastAcceptedTotpStep = null;
            var resetFailures = await users.ResetAccessFailedCountAsync(user);
            if (!resetFailures.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Error(500, "admin_mfa_reset_failed");
            }
            user.LockoutEnd = null;

            var activeSessions = await db.Sessions
                .Where(session => session.UserId == user.Id && session.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);
            var now = DateTimeOffset.UtcNow;
            foreach (var session in activeSessions) session.Revoke(now);

            db.SecurityAuditRecords.Add(new SecurityAuditRecord
            {
                SubjectUserId = user.Id,
                EventCode = "admin_mfa_reset",
                ResultCode = "succeeded",
                TraceId = http.TraceIdentifier,
                OccurredAtUtc = now
            });
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var label = Uri.EscapeDataString($"TurboRent:{user.Email}");
            var issuer = Uri.EscapeDataString("TurboRent");
            var authenticatorUri =
                $"otpauth://totp/{label}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
            return Results.Ok(new { email = user.Email, authenticatorSecret = secret, authenticatorUri });
        }).RequireRateLimiting(RateLimitPolicies.Identity);
        return endpoints;
    }


    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record TotpResetRequest(string? BootstrapSecret, string? Email);
}
