using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Modules.Identity.Features.VerifyEmail;

public sealed class VerifyEmailHandler(IdentityDbContext db, UserManager<ApplicationUser> users)
{
    public async Task<bool> Handle(Guid userId, string token, string traceId, CancellationToken ct)
    {
        string decoded;
        try { decoded = System.Text.Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)); }
        catch (FormatException) { return false; }
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Row lock serializes token confirmation and avoids concurrent stale writes.
        var user = await db.Users.FromSqlInterpolated(
            $"SELECT * FROM identity.users WHERE \"Id\" = {userId} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (user is null) return false;
        // Verify the supplied token even when already confirmed; invalid tokens never succeed.
        if (!await users.VerifyUserTokenAsync(user, users.Options.Tokens.EmailConfirmationTokenProvider,
            UserManager<ApplicationUser>.ConfirmEmailTokenPurpose, decoded)) return false;
        if (user.EmailConfirmed) return true;
        var result = await users.ConfirmEmailAsync(user, decoded);
        if (!result.Succeeded) return false;
        db.SecurityAuditRecords.Add(new SecurityAuditRecord
        { ActorUserId = user.Id, SubjectUserId = user.Id, EventCode = "email_verification", ResultCode = "confirmed", TraceId = traceId, OccurredAtUtc = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }
}
