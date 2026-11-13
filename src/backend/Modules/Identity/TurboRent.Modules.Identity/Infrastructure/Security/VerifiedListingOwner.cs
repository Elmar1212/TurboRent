using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TurboRent.Modules.Identity.Contracts;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public sealed class VerifiedListingOwner(IdentityDbContext db, SessionAuthenticator sessions) : IVerifiedListingOwner
{
    public async Task<ListingOwnerAccess?> FindListingOwnerAccessAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var active = await sessions.AuthenticateAsync(context, cancellationToken);
        if (active is null) return null;

        var userId = active.User.Id;
        var ownerStatus = await db.OwnerProfiles.AsNoTracking().Where(profile => profile.UserId == userId)
            .Select(profile => (ApprovalStatus?)profile.Status).SingleOrDefaultAsync(cancellationToken);
        var companyStatus = await db.CompanyProfiles.AsNoTracking().Where(profile => profile.UserId == userId)
            .Select(profile => (ApprovalStatus?)profile.Status).SingleOrDefaultAsync(cancellationToken);
        var hasOwnerProfile = ownerStatus.HasValue || companyStatus.HasValue;
        var isApproved = ownerStatus == ApprovalStatus.Approved || companyStatus == ApprovalStatus.Approved;
        return new ListingOwnerAccess(userId, hasOwnerProfile, isApproved);
    }
}
