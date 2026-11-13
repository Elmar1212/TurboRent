using Microsoft.AspNetCore.Http;
using TurboRent.Modules.Identity.Contracts;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public sealed class AuthenticatedUserResolver(SessionAuthenticator sessions) : IAuthenticatedUser
{
    public async Task<AuthenticatedUser?> FindAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var active = await sessions.AuthenticateAsync(context, cancellationToken);
        return active is null
            ? null
            : new AuthenticatedUser(active.User.Id, active.User.DisplayName, active.User.Locale);
    }
}
