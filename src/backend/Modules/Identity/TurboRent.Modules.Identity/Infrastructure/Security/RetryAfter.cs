using System.Globalization;
using System.Threading.RateLimiting;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public static class RetryAfter
{
    public static string Seconds(RateLimitLease lease, TimeSpan fallback)
    {
        var delay = lease.TryGetMetadata(MetadataName.RetryAfter, out var estimated) ? estimated : fallback;
        var seconds = Math.Clamp(Math.Ceiling(delay.TotalSeconds), 1, int.MaxValue);
        return seconds.ToString(CultureInfo.InvariantCulture);
    }
}
