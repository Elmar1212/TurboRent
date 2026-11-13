namespace TurboRent.Modules.Identity.Infrastructure.Email;

/// <param name="LinkName">Machine name of the primary link, for example <c>verificationUrl</c>.</param>
public sealed record EmailMessage(
    string ToName, string ToEmail, string Subject, string Body, string Locale,
    string Kind, string LinkName, string LinkUrl);

/// <summary>Delivers a composed message. Implementations throw on delivery failure.</summary>
public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

/// <summary>Configuration shared by the email transports.</summary>
public static class EmailSettings
{
    public const string SmtpHostKey = "Email:Smtp:Host";
    public const string FromAddressKey = "Email:FromAddress";
    public const string FromNameKey = "Email:FromName";

    public static string FromAddress(Microsoft.Extensions.Configuration.IConfiguration configuration) =>
        configuration[FromAddressKey] ?? "no-reply@turborent.local";

    public static string FromName(Microsoft.Extensions.Configuration.IConfiguration configuration) =>
        configuration[FromNameKey] ?? "TurboRent";
}
