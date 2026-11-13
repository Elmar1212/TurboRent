using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Email;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Modules.Identity.Features.Register;

public sealed class RegisterHandler(
    IdentityDbContext db, UserManager<ApplicationUser> users, IIdentityEmail email,
    ILogger<RegisterHandler> logger)
{
    public async Task<bool> Handle(RegisterRequest request, string traceId, CancellationToken ct)
    {
        ApplicationUser? created = null;
        await using (var transaction = await db.Database.BeginTransactionAsync(ct))
        {
            var normalized = users.NormalizeEmail(request.Email!);
            if (await db.Users.AnyAsync(user => user.NormalizedEmail == normalized, ct))
            {
                // Perform the same expensive password hashing on existing accounts to reduce timing leakage.
                _ = users.PasswordHasher.HashPassword(new ApplicationUser(), request.Password!);
                return true;
            }
            var now = DateTimeOffset.UtcNow;
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(), UserName = request.Email, Email = request.Email,
                DisplayName = request.DisplayName!.Trim(), Locale = request.Locale!, CreatedAtUtc = now,
                EmailConfirmed = false, LockoutEnabled = true
            };
            try
            {
                var result = await users.CreateAsync(user, request.Password!);
                if (!result.Succeeded)
                {
                    if (result.Errors.All(error => error.Code is "DuplicateEmail" or "DuplicateUserName")) return true;
                    throw new InvalidOperationException("Validated registration could not be persisted.");
                }
                var role = await users.AddToRoleAsync(user, IdentityRoles.Customer);
                if (!role.Succeeded) throw new InvalidOperationException("Customer role assignment failed.");
                if (request.AccountType == "owner")
                    db.OwnerProfiles.Add(new OwnerProfile { UserId = user.Id, ContactPhone = request.ContactPhone!, SubmittedAtUtc = now });
                if (request.AccountType == "company")
                    db.CompanyProfiles.Add(new CompanyProfile
                    { UserId = user.Id, LegalName = request.LegalName!.Trim(), Voen = request.Voen!, ContactPhone = request.ContactPhone!, SubmittedAtUtc = now });
                db.SecurityAuditRecords.Add(new SecurityAuditRecord
                { ActorUserId = user.Id, SubjectUserId = user.Id, EventCode = "registration", ResultCode = "created", TraceId = traceId, OccurredAtUtc = now });
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                created = user;
            }
            catch (DbUpdateException exception) when (
                exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: "EmailIndex" or "UserNameIndex" })
            {
                await transaction.RollbackAsync(ct);
                return true; // A concurrent duplicate gets the same public response.
            }
        }
        return created is null || await SendVerification(created, traceId, ct);
    }

    public async Task<bool> SendVerification(ApplicationUser user, string traceId, CancellationToken ct)
    {
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Could not generate email verification token.");
        try { await email.SendVerificationAsync(user, token, ct); }
        catch (Exception exception) when (IdentityEmail.IsDeliveryFailure(exception))
        {
            // Never log exception details, which may contain local paths, or any verification data.
            logger.LogWarning("Verification delivery failed. FailureType: {FailureType}. TraceId: {TraceId}",
                exception.GetType().Name, traceId);
            return false;
        }
        return true;
    }
}
