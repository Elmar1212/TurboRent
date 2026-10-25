namespace TurboRent.Modules.System.Features.GetSystemStatus;

public sealed record SystemStatus(string Status, string Service, string Currency, string TimeZone)
{
    public static SystemStatus Healthy() => new("healthy", "TurboRent API", "AZN", "Asia/Baku");
}
