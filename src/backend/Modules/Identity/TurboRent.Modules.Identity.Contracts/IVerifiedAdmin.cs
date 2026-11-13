using Microsoft.AspNetCore.Http;

namespace TurboRent.Modules.Identity.Contracts;

public interface IVerifiedAdmin
{
    Task<Guid?> FindVerifiedAdminAsync(HttpContext context, CancellationToken cancellationToken);
}
