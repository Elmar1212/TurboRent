namespace TurboRent.BuildingBlocks.Http;

/// <summary>Named rate limit policies registered by the API host.</summary>
public static class RateLimitPolicies
{
    /// <summary>Authentication writes: login, registration, password reset.</summary>
    public const string Identity = "identity-ip";
    /// <summary>Signed-in writes such as listing uploads and booking requests.</summary>
    public const string Writes = "writes-ip";
    /// <summary>Admin and owner dashboards, which load many images per page.</summary>
    public const string Dashboard = "dashboard-ip";
    /// <summary>Revealing an owner's phone number.</summary>
    public const string Contact = "contact-ip";
}
