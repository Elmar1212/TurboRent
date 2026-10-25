using TurboRent.Modules.System.Features.GetSystemStatus;

namespace TurboRent.UnitTests;
public class SystemStatusTests
{
    [Fact]
    public void Healthy_status_identifies_market_defaults()
    {
        var status = SystemStatus.Healthy();
        Assert.Equal("healthy", status.Status);
        Assert.Equal("AZN", status.Currency);
        Assert.Equal("Asia/Baku", status.TimeZone);
    }
}
