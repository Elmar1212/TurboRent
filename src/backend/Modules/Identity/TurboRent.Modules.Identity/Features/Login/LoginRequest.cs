using System.Text.Json.Serialization;

namespace TurboRent.Modules.Identity.Features.Login;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record LoginRequest(string? Email, string? Password, string? TotpCode = null);
