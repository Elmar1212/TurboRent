using Microsoft.AspNetCore.Http;

namespace TurboRent.Modules.Identity.Contracts;

public interface IVerifiedListingOwner
{
    Task<ListingOwnerAccess?> FindListingOwnerAccessAsync(HttpContext context, CancellationToken cancellationToken);
}

public sealed record ListingOwnerAccess(Guid UserId, bool HasOwnerProfile, bool IsApproved);
