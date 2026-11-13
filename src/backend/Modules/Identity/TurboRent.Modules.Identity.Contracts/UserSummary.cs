namespace TurboRent.Modules.Identity.Contracts;

// This contract deliberately excludes password, session and private profile data.
public sealed record UserSummary(Guid Id, string DisplayName);
public interface IIdentityQueries
{
    Task<UserSummary?> FindUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
