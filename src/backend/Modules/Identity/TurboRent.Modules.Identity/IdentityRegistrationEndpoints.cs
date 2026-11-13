using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Features.Login;
using TurboRent.Modules.Identity.Features.Register;
using TurboRent.Modules.Identity.Features.VerifyEmail;
using TurboRent.Modules.Identity.Infrastructure.Persistence;
using TurboRent.Modules.Identity.Infrastructure.Security;
using TurboRent.Modules.Identity.Validation;
using TurboRent.Modules.Identity.Features.Password;
using TurboRent.BuildingBlocks.Http;
using static TurboRent.BuildingBlocks.Http.ApiProblem;

namespace TurboRent.Modules.Identity;

public static class IdentityRegistrationEndpoints
{
    public static IEndpointRouteBuilder MapIdentityRegistration(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Registration");
        group.MapGet("/csrf", (HttpContext http, IAntiforgery antiforgery) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            if (!http.Request.IsHttps) return Error(403, "https_required");
            return Results.Ok(new { requestToken = antiforgery.GetAndStoreTokens(http).RequestToken });
        });
        group.MapGet("/session", async (HttpContext http, IdentityDbContext db, ApplicationSessionCookie cookie,
            SessionAuthenticator sessions) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            if (!http.Request.IsHttps) return Error(403, "https_required");
            // Each check also extends the idle timeout, so active users stay signed in.
            var session = await sessions.AuthenticateAsync(http, http.RequestAborted);
            if (session is null)
            {
                cookie.Clear(http);
                return Error(401, "not_authenticated");
            }
            var approval = await GetAccountApprovalAsync(db, session.User.Id, http.RequestAborted);
            return Results.Ok(new
            {
                displayName = session.User.DisplayName,
                email = session.User.Email,
                locale = session.User.Locale,
                hasOwnerProfile = approval.Status is not null,
                canCreateListing = approval.Status == ApprovalStatus.Approved,
                accountApprovalStatus = approval.Status?.ToString(),
                accountApprovalReason = approval.ReasonCode,
                isAdmin = session.Session.IsAdminSession && session.Session.MfaVerifiedAtUtc is not null
            });
        });
        var writes = group.MapGroup("").RequireRateLimiting(RateLimitPolicies.Identity);
        writes.AddEndpointFilter(async (context, next) =>
        {
            var http = context.HttpContext;
            http.Response.Headers.CacheControl = "no-store";
            http.Response.Headers["Referrer-Policy"] = "no-referrer";
            if (!http.Request.IsHttps) return Error(403, "https_required");
            if (http.Request.ContentLength > 16384) return Error(413, "request_too_large");
            try { await http.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(http); }
            catch (AntiforgeryValidationException) { return Error(400, "invalid_csrf"); }
            return await next(context);
        });
        writes.MapPost("/register", async (RegisterRequest request, HttpContext http,
            RegisterHandler handler, IdentityLimits limits, UserManager<ApplicationUser> users) =>
        {
            var errors = RegistrationValidation.Validate(request);
            if (errors.Count > 0) return Results.ValidationProblem(errors, extensions: new Dictionary<string, object?> { ["code"] = "validation_failed" });
            using var lease = limits.Acquire("register", users.NormalizeEmail(request.Email!)!);
            if (!lease.IsAcquired) { http.Response.Headers.RetryAfter = RetryAfter.Seconds(lease, TimeSpan.FromMinutes(15)); return Error(429, "rate_limited"); }
            var delivered = await handler.Handle(request, http.TraceIdentifier, http.RequestAborted);
            return delivered ? Accepted() : Error(503, "local_mailbox_unavailable");
        });
        writes.MapPost("/login", async (LoginRequest request, HttpContext http, LoginHandler handler,
            UserManager<ApplicationUser> users, IdentityLimits limits, ApplicationSessionCookie cookie,
            IdentityDbContext db) =>
        {
            if (!RegistrationValidation.ValidEmail(request.Email) ||
                request.Password is not { Length: > 0 and <= 128 } ||
                request.TotpCode is { Length: > 0 and not 6 })
                return Error(401, "invalid_credentials");
            using var lease = limits.Acquire("login", users.NormalizeEmail(request.Email!)!);
            if (!lease.IsAcquired)
            {
                http.Response.Headers.RetryAfter = RetryAfter.Seconds(lease, IdentityLimits.LoginWindow);
                return Error(429, "rate_limited");
            }
            var result = await handler.Handle(request, http.TraceIdentifier, http.RequestAborted);
            if (!result.IsSuccess)
                return Error(result.Code == "mfa_required" ? 403 : 401, result.Code!);
            cookie.Set(http, result.Session!.Id, result.Session.AbsoluteExpiresAtUtc);
            var approval = await GetAccountApprovalAsync(db, result.User!.Id, http.RequestAborted);
            return Results.Ok(new
            {
                displayName = result.User.DisplayName,
                isAdmin = result.Session.IsAdminSession,
                canCreateListing = approval.Status == ApprovalStatus.Approved,
                accountApprovalStatus = approval.Status?.ToString()
            });
        });
        writes.MapPost("/logout", async (HttpContext http, IdentityDbContext db, ApplicationSessionCookie cookie) =>
        {
            if (cookie.TryUnprotect(http.Request.Cookies[ApplicationSessionCookie.Name], out var sessionId))
            {
                var session = await db.Sessions.SingleOrDefaultAsync(item => item.Id == sessionId, http.RequestAborted);
                if (session is not null && session.RevokedAtUtc is null)
                {
                    session.Revoke(DateTimeOffset.UtcNow);
                    await db.SaveChangesAsync(http.RequestAborted);
                }
            }
            cookie.Clear(http);
            return Results.NoContent();
        });
        writes.MapPost("/email/resend", async (EmailRequest request, HttpContext http,
            UserManager<ApplicationUser> users, RegisterHandler handler, IdentityLimits limits) =>
        {
            if (!RegistrationValidation.ValidEmail(request.Email)) return Error(400, "invalid_email");
            using var lease = limits.Acquire("resend", users.NormalizeEmail(request.Email!)!);
            if (!lease.IsAcquired) { http.Response.Headers.RetryAfter = RetryAfter.Seconds(lease, TimeSpan.FromMinutes(15)); return Error(429, "rate_limited"); }
            var user = await users.FindByEmailAsync(request.Email!);
            if (user is { EmailConfirmed: false, AccountStatus: "Active" })
            {
                var delivered = await handler.SendVerification(user, http.TraceIdentifier, http.RequestAborted);
                if (!delivered) return Error(503, "local_mailbox_unavailable");
            }
            return Accepted();
        });
        writes.MapPost("/email/verify", async (VerifyEmailRequest request, HttpContext http, VerifyEmailHandler handler) =>
        {
            if (request.UserId == Guid.Empty || request.Token is not { Length: > 0 and <= 4096 }) return Error(400, "invalid_verification");
            var verified = await handler.Handle(request.UserId, request.Token, http.TraceIdentifier, http.RequestAborted);
            if (!verified) return Error(400, "invalid_verification");
            var hasOwnerProfile = await http.RequestServices.GetRequiredService<IdentityDbContext>()
                .OwnerProfiles.AnyAsync(profile => profile.UserId == request.UserId, http.RequestAborted);
            var hasCompanyProfile = await http.RequestServices.GetRequiredService<IdentityDbContext>()
                .CompanyProfiles.AnyAsync(profile => profile.UserId == request.UserId, http.RequestAborted);
            return Results.Ok(new { code = "email_verified", requiresListingSetup = hasOwnerProfile || hasCompanyProfile });
        });
        writes.MapPasswordEndpoints();
        return endpoints;
    }

    private static IResult Accepted() => Results.Accepted(value: new { code = "verification_requested" });

    private static async Task<AccountApprovalSummary> GetAccountApprovalAsync(
        IdentityDbContext db, Guid userId, CancellationToken cancellationToken)
    {
        var owner = await db.OwnerProfiles.AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => new AccountApprovalSummary(profile.Status, profile.ReasonCode))
            .SingleOrDefaultAsync(cancellationToken);
        if (owner is not null) return owner;
        return await db.CompanyProfiles.AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => new AccountApprovalSummary(profile.Status, profile.ReasonCode))
            .SingleOrDefaultAsync(cancellationToken) ?? new AccountApprovalSummary(null, null);
    }

    private sealed record AccountApprovalSummary(ApprovalStatus? Status, string? ReasonCode);
}
