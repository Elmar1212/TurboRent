namespace TurboRent.Modules.Identity.Contracts;

public interface IListingOwnerDirectory
{
    Task<IReadOnlyDictionary<Guid, ListingOwnerIdentity>> FindAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);
}

/// <summary>
/// Owner details shown to admins and, for approved listings, to renters.
/// <paramref name="ContactPhone"/> is the number the owner supplied for listing contact.
/// </summary>
public sealed record ListingOwnerIdentity(
    string DisplayName, string? Email, string? ContactPhone, string? CompanyName);
