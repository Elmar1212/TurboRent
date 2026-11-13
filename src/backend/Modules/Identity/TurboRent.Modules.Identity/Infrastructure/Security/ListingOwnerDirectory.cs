using Microsoft.EntityFrameworkCore;
using TurboRent.Modules.Identity.Contracts;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public sealed class ListingOwnerDirectory(IdentityDbContext db) : IListingOwnerDirectory
{
    public async Task<IReadOnlyDictionary<Guid, ListingOwnerIdentity>> FindAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0) return new Dictionary<Guid, ListingOwnerIdentity>();
        var users = await db.Users.AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new { user.Id, user.DisplayName, user.Email })
            .ToListAsync(cancellationToken);
        var owners = await db.OwnerProfiles.AsNoTracking()
            .Where(profile => userIds.Contains(profile.UserId))
            .Select(profile => new { profile.UserId, profile.ContactPhone })
            .ToDictionaryAsync(profile => profile.UserId, profile => profile.ContactPhone, cancellationToken);
        var companies = await db.CompanyProfiles.AsNoTracking()
            .Where(profile => userIds.Contains(profile.UserId))
            .Select(profile => new { profile.UserId, profile.ContactPhone, profile.LegalName })
            .ToDictionaryAsync(profile => profile.UserId, cancellationToken);
        return users.ToDictionary(
            user => user.Id,
            user =>
            {
                companies.TryGetValue(user.Id, out var company);
                var phone = company?.ContactPhone ?? owners.GetValueOrDefault(user.Id);
                return new ListingOwnerIdentity(user.DisplayName, user.Email, phone, company?.LegalName);
            });
    }
}
