using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;
using TurboRent.Modules.Identity.Contracts;
using TurboRent.BuildingBlocks.Http;
using static TurboRent.BuildingBlocks.Http.ApiProblem;

namespace TurboRent.Modules.Identity.Features.AdminApprovals;

public static class AdminAccountApprovalEndpoints
{
    private static readonly HashSet<string> RejectionReasons =
        ["incomplete_information", "invalid_contact_details", "invalid_business_information", "other"];

    public static IEndpointRouteBuilder MapAdminAccountApprovals(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/accounts")
            .WithTags("Admin account approval")
            .RequireRateLimiting(RateLimitPolicies.Dashboard);

        group.MapGet("", async (
            int? page,
            int? pageSize,
            HttpContext http,
            IVerifiedAdmin verifiedAdmin,
            IdentityDbContext db,
            CancellationToken cancellationToken) =>
        {
            http.Response.Headers.CacheControl = "no-store";
            if (await verifiedAdmin.FindVerifiedAdminAsync(http, cancellationToken) is null)
                return Error(401, "admin_sign_in_required");
            var currentPage = page ?? 1;
            var currentPageSize = pageSize ?? 20;
            if (currentPage is < 1 or > 10000 || currentPageSize is < 1 or > 50)
                return Error(400, "invalid_admin_query");

            var owners = db.OwnerProfiles.AsNoTracking()
                .Where(profile => profile.Status == ApprovalStatus.Pending)
                .Join(db.Users.AsNoTracking().Where(user => user.EmailConfirmed && user.AccountStatus == "Active"),
                    profile => profile.UserId, user => user.Id,
                    (profile, user) => new
                    {
                        UserId = user.Id,
                        AccountType = "owner",
                        user.DisplayName,
                        Email = user.Email!,
                        profile.ContactPhone,
                        LegalName = (string?)null,
                        Voen = (string?)null,
                        profile.SubmittedAtUtc
                    });
            var companies = db.CompanyProfiles.AsNoTracking()
                .Where(profile => profile.Status == ApprovalStatus.Pending)
                .Join(db.Users.AsNoTracking().Where(user => user.EmailConfirmed && user.AccountStatus == "Active"),
                    profile => profile.UserId, user => user.Id,
                    (profile, user) => new
                    {
                        UserId = user.Id,
                        AccountType = "company",
                        user.DisplayName,
                        Email = user.Email!,
                        profile.ContactPhone,
                        LegalName = (string?)profile.LegalName,
                        Voen = (string?)profile.Voen,
                        profile.SubmittedAtUtc
                    });
            var pending = owners.Concat(companies);
            var total = await pending.CountAsync(cancellationToken);
            var items = await pending
                .OrderBy(account => account.SubmittedAtUtc).ThenBy(account => account.UserId)
                .Skip((currentPage - 1) * currentPageSize).Take(currentPageSize)
                .Select(account => new PendingAdminAccountResponse(
                    account.UserId, account.AccountType, account.DisplayName, account.Email,
                    account.ContactPhone, account.LegalName, account.Voen, account.SubmittedAtUtc))
                .ToListAsync(cancellationToken);
            return Results.Ok(new PendingAdminAccountPageResponse(items, total, currentPage, currentPageSize));
        })
            .WithName("GetPendingAdminAccounts")
            .WithSummary("List owner and company accounts waiting for Admin approval.")
            .Produces<PendingAdminAccountPageResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/{userId:guid}/approve", async (
            Guid userId,
            HttpContext http,
            IAntiforgery antiforgery,
            IVerifiedAdmin verifiedAdmin,
            IdentityDbContext db,
            CancellationToken cancellationToken) =>
        {
            var authorization = await ValidateWriteAsync(http, antiforgery, verifiedAdmin, cancellationToken);
            var adminId = authorization.AdminId;
            if (adminId is null)
                return Error(authorization.ErrorCode == "invalid_csrf" ? 400 :
                    authorization.ErrorCode == "https_required" ? 403 : 401, authorization.ErrorCode!);
            return await ReviewAsync(userId, ApprovalStatus.Approved, null, adminId.Value,
                http.TraceIdentifier, db, cancellationToken);
        })
            .WithName("ApproveAdminAccount")
            .WithSummary("Approve a pending owner or company account.")
            .Produces<AccountApprovalResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{userId:guid}/reject", async (
            Guid userId,
            RejectAdminAccountRequest request,
            HttpContext http,
            IAntiforgery antiforgery,
            IVerifiedAdmin verifiedAdmin,
            IdentityDbContext db,
            CancellationToken cancellationToken) =>
        {
            var authorization = await ValidateWriteAsync(http, antiforgery, verifiedAdmin, cancellationToken);
            var adminId = authorization.AdminId;
            if (adminId is null)
                return Error(authorization.ErrorCode == "invalid_csrf" ? 400 :
                    authorization.ErrorCode == "https_required" ? 403 : 401, authorization.ErrorCode!);
            if (request.ReasonCode is null || !RejectionReasons.Contains(request.ReasonCode))
                return Error(400, "invalid_rejection_reason");
            return await ReviewAsync(userId, ApprovalStatus.Rejected, request.ReasonCode, adminId.Value,
                http.TraceIdentifier, db, cancellationToken);
        })
            .WithName("RejectAdminAccount")
            .WithSummary("Reject a pending owner or company account with a reason.")
            .Produces<AccountApprovalResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> ReviewAsync(
        Guid userId,
        ApprovalStatus decision,
        string? reasonCode,
        Guid adminId,
        string traceId,
        IdentityDbContext db,
        CancellationToken cancellationToken)
    {
        var eligibleUser = await db.Users.AnyAsync(user =>
            user.Id == userId && user.EmailConfirmed && user.AccountStatus == "Active", cancellationToken);
        if (!eligibleUser) return Results.NotFound();
        var owner = await db.OwnerProfiles.SingleOrDefaultAsync(
            profile => profile.UserId == userId && profile.Status == ApprovalStatus.Pending, cancellationToken);
        var company = await db.CompanyProfiles.SingleOrDefaultAsync(
            profile => profile.UserId == userId && profile.Status == ApprovalStatus.Pending, cancellationToken);
        if (owner is not null && company is not null)
            return Error(409, "ambiguous_account_profile");
        if (owner is null && company is null)
        {
            var accountExists = await db.OwnerProfiles.AnyAsync(profile => profile.UserId == userId, cancellationToken) ||
                await db.CompanyProfiles.AnyAsync(profile => profile.UserId == userId, cancellationToken);
            return accountExists ? Error(409, "account_already_reviewed") : Results.NotFound();
        }

        var accountType = owner is not null ? "owner" : "company";
        if (reasonCode == "invalid_business_information" && accountType != "company")
            return Error(400, "invalid_rejection_reason");

        try
        {
            if (owner is not null)
                owner.Review(decision, adminId, reasonCode, DateTimeOffset.UtcNow);
            else
                company!.Review(decision, adminId, reasonCode, DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException)
        {
            return Error(409, "account_review_conflict");
        }

        db.SecurityAuditRecords.Add(new SecurityAuditRecord
        {
            ActorUserId = adminId,
            SubjectUserId = userId,
            EventCode = "account_review",
            ResultCode = decision == ApprovalStatus.Approved ? "approved" : "rejected",
            TraceId = traceId,
            OccurredAtUtc = DateTimeOffset.UtcNow
        });
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return Error(409, "account_already_reviewed"); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_company_profiles_Voen" })
        {
            return Error(409, "company_voen_already_approved");
        }
        return Results.Ok(new AccountApprovalResponse(userId, accountType, decision.ToString()));
    }

    private static async Task<(Guid? AdminId, string? ErrorCode)> ValidateWriteAsync(
        HttpContext http,
        IAntiforgery antiforgery,
        IVerifiedAdmin verifiedAdmin,
        CancellationToken cancellationToken)
    {
        http.Response.Headers.CacheControl = "no-store";
        http.Response.Headers["Referrer-Policy"] = "no-referrer";
        if (!http.Request.IsHttps) return (null, "https_required");
        try { await antiforgery.ValidateRequestAsync(http); }
        catch (AntiforgeryValidationException) { return (null, "invalid_csrf"); }
        var adminId = await verifiedAdmin.FindVerifiedAdminAsync(http, cancellationToken);
        return adminId is null ? (null, "admin_sign_in_required") : (adminId, null);
    }


    /// <summary>A pending owner or company account shown to an Admin reviewer.</summary>
    public sealed record PendingAdminAccountResponse(
        Guid UserId,
        string AccountType,
        string DisplayName,
        string Email,
        string ContactPhone,
        string? LegalName,
        string? Voen,
        DateTimeOffset SubmittedAtUtc);

    /// <summary>A page of owner and company accounts waiting for review.</summary>
    public sealed record PendingAdminAccountPageResponse(
        IReadOnlyList<PendingAdminAccountResponse> Items,
        int Total,
        int Page,
        int PageSize);

    /// <summary>The result of an Admin account review.</summary>
    public sealed record AccountApprovalResponse(Guid UserId, string AccountType, string Status);

    /// <summary>Reason code required when rejecting an owner or company account.</summary>
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record RejectAdminAccountRequest(string? ReasonCode);
}
