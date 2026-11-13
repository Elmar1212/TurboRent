using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public sealed record ActiveSession(ApplicationSession Session, ApplicationUser User);

/// <summary>
/// Validates the session cookie and slides the idle timeout while the user is active.
/// The result is cached per request so several checks cost a single database round trip.
/// </summary>
public sealed class SessionAuthenticator(IdentityDbContext db, ApplicationSessionCookie cookie, TimeProvider time)
{
    /// <summary>Activity refreshes the idle timeout at most this often to limit database writes.</summary>
    public static readonly TimeSpan TouchInterval = TimeSpan.FromMinutes(1);
    private static readonly object CacheKey = new();

    public async Task<ActiveSession?> AuthenticateAsync(HttpContext http, CancellationToken cancellationToken)
    {
        if (http.Items.TryGetValue(CacheKey, out var cached)) return cached as ActiveSession;
        var result = await LoadAsync(http, cancellationToken);
        http.Items[CacheKey] = result;
        return result;
    }

    private async Task<ActiveSession?> LoadAsync(HttpContext http, CancellationToken cancellationToken)
    {
        if (!http.Request.IsHttps ||
            !cookie.TryUnprotect(http.Request.Cookies[ApplicationSessionCookie.Name], out var sessionId))
            return null;

        var row = await db.Sessions
            .Where(item => item.Id == sessionId)
            .Join(db.Users, item => item.UserId, user => user.Id,
                (item, user) => new { Session = item, User = user })
            .SingleOrDefaultAsync(cancellationToken);
        var now = time.GetUtcNow();
        if (row is null || !row.Session.IsActive(now) ||
            !row.User.EmailConfirmed || row.User.AccountStatus != "Active")
            return null;

        if (now - row.Session.LastSeenAtUtc >= TouchInterval)
        {
            row.Session.Touch(now);
            try { await db.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateConcurrencyException)
            {
                // A parallel request refreshed or revoked the session first; use the stored state.
                await db.Entry(row.Session).ReloadAsync(cancellationToken);
                if (!row.Session.IsActive(now)) return null;
            }
        }
        return new ActiveSession(row.Session, row.User);
    }
}
