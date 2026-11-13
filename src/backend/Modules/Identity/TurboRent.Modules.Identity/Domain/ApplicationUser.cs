using Microsoft.AspNetCore.Identity;

namespace TurboRent.Modules.Identity.Domain;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = "";
    public string Locale { get; set; } = "az";
    public string AccountStatus { get; set; } = "Active";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public long? LastAcceptedTotpStep { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>;

public static class IdentityRoles
{
    public const string Customer = "Customer";
    public const string IndividualOwner = "IndividualOwner";
    public const string RentalCompany = "RentalCompany";
    public const string Admin = "Admin";

    // Stable identifiers make migration generation deterministic. These are roles, not users.
    public static readonly Guid CustomerId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid OwnerId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    public static readonly Guid CompanyId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    public static readonly Guid AdminId = Guid.Parse("44444444-4444-4444-8444-444444444444");
}
