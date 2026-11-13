using System.Text.Json.Serialization;

namespace TurboRent.Modules.Identity.Features.Register;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record RegisterRequest(string? Email, string? Password, string? DisplayName,
    string? Locale, string? AccountType, string? ContactPhone, string? LegalName, string? Voen);
public sealed record EmailRequest(string? Email);
public sealed record VerifyEmailRequest(Guid UserId, string? Token);
