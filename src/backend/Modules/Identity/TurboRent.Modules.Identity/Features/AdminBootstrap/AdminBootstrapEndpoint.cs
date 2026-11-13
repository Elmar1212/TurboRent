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

public static class AdminBootstrapEndpoint
{
    public static IEndpointRouteBuilder MapLocalAdminBootstrap(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/internal/admin/bootstrap", async (
            BootstrapRequest request,
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

            if (!RegistrationValidation.ValidEmail(request.Email) ||
                request.Password is not { Length: >= 15 and <= 128 } ||
                request.DisplayName is not { Length: >= 1 and <= 100 } ||
                request.DisplayName.Trim().Length == 0 ||
                request.Locale is not ("az" or "en"))
                return Error(400, "invalid_admin_bootstrap");

            await using var transaction = await db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var hasAdmin = await db.UserRoles.Join(db.Roles.Where(role => role.Name == IdentityRoles.Admin),
                    assignment => assignment.RoleId, role => role.Id, (assignment, _) => assignment.UserId)
                .AnyAsync(cancellationToken);
            var alreadyBootstrapped = await db.SecurityAuditRecords.AnyAsync(
                record => record.EventCode == "admin_bootstrap", cancellationToken);
            if (hasAdmin || alreadyBootstrapped) return Error(409, "admin_bootstrap_closed");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true,
                DisplayName = request.DisplayName.Trim(),
                Locale = request.Locale,
                AccountStatus = "Active",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            if (!(await users.CreateAsync(user, request.Password)).Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Error(400, "admin_bootstrap_failed");
            }
            if (!(await users.AddToRoleAsync(user, IdentityRoles.Admin)).Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Error(400, "admin_bootstrap_failed");
            }

            var secret = AdminTotpService.GenerateSecret();
            if (!(await totp.SetSecretAsync(user, secret)).Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Error(500, "admin_bootstrap_failed");
            }
            db.SecurityAuditRecords.Add(new SecurityAuditRecord
            {
                ActorUserId = user.Id,
                SubjectUserId = user.Id,
                EventCode = "admin_bootstrap",
                ResultCode = "succeeded",
                TraceId = http.TraceIdentifier,
                OccurredAtUtc = DateTimeOffset.UtcNow
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
    public sealed record BootstrapRequest(
        string? BootstrapSecret,
        string? Email,
        string? Password,
        string? DisplayName,
        string? Locale);
}
