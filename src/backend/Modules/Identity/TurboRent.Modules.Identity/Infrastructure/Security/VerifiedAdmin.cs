using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TurboRent.Modules.Identity.Contracts;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public sealed class VerifiedAdmin(IdentityDbContext db, SessionAuthenticator sessions) : IVerifiedAdmin
{
    public async Task<Guid?> FindVerifiedAdminAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var active = await sessions.AuthenticateAsync(context, cancellationToken);
        if (active is null || !active.Session.IsAdminSession || active.Session.MfaVerifiedAtUtc is null)
            return null;

        // The role is re-checked on every request so revoking it takes effect immediately.
        var hasAdminRole = await db.UserRoles.AsNoTracking()
            .Where(assignment => assignment.UserId == active.User.Id)
            .Join(db.Roles.AsNoTracking().Where(role => role.Name == IdentityRoles.Admin),
                assignment => assignment.RoleId, role => role.Id, (_, _) => true)
            .AnyAsync(cancellationToken);
        return hasAdminRole ? active.User.Id : null;
    }
}
