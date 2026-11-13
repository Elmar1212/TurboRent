using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public sealed class IdentityLimits : IDisposable
{
    public static readonly TimeSpan LoginWindow = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan DefaultWindow = TimeSpan.FromMinutes(15);

    private readonly PartitionedRateLimiter<string> accounts = PartitionedRateLimiter.Create<string, string>(key =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = key.StartsWith("login:", StringComparison.Ordinal) ? LoginWindow : DefaultWindow,
            QueueLimit = 0,
            AutoReplenishment = true
        }));

    public RateLimitLease Acquire(string operation, string normalizedEmail) =>
        accounts.AttemptAcquire(operation + ":" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedEmail))));
    public void Dispose() => accounts.Dispose();
}
