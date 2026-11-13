using Microsoft.AspNetCore.Http;

namespace TurboRent.Modules.Identity.Contracts;

/// <summary>Resolves the signed-in user from the server-side session cookie.</summary>
public interface IAuthenticatedUser
{
    /// <summary>Returns null when the request has no active session for a verified, active account.</summary>
    Task<AuthenticatedUser?> FindAsync(HttpContext context, CancellationToken cancellationToken);
}

public sealed record AuthenticatedUser(Guid UserId, string DisplayName, string Locale);
