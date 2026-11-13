using System.Globalization;
using System.Threading.RateLimiting;
using TurboRent.Modules.Identity.Infrastructure.Security;

namespace TurboRent.Identity.UnitTests;

public class RetryAfterTests
{
    [Fact]
    public void Fixed_window_metadata_is_used_instead_of_full_window_fallback()
    {
        using var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        { PermitLimit = 1, Window = TimeSpan.FromSeconds(15), QueueLimit = 0, AutoReplenishment = true });
        using var accepted = limiter.AttemptAcquire();
        using var rejected = limiter.AttemptAcquire();
        Assert.False(rejected.IsAcquired);
        var seconds = int.Parse(RetryAfter.Seconds(rejected, TimeSpan.FromMinutes(15)), CultureInfo.InvariantCulture);
        Assert.InRange(seconds, 1, 15);
    }

    [Fact]
    public void Limiter_without_retry_metadata_uses_existing_policy_fallback()
    {
        using var limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions { PermitLimit = 1, QueueLimit = 0 });
        using var accepted = limiter.AttemptAcquire();
        using var rejected = limiter.AttemptAcquire();
        Assert.False(rejected.IsAcquired);
        Assert.Equal("900", RetryAfter.Seconds(rejected, TimeSpan.FromMinutes(15)));
    }
}
