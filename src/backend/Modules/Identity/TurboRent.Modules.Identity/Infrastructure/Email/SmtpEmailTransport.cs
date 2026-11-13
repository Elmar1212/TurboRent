using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace TurboRent.Modules.Identity.Infrastructure.Email;

/// <summary>
/// Sends real email through an SMTP relay (for example Amazon SES, Mailgun, SendGrid or Postmark).
/// Configure <c>Email:Smtp:Host</c>, <c>Port</c>, <c>Username</c>, <c>Password</c> and <c>Email:FromAddress</c>.
/// </summary>
public sealed class SmtpEmailTransport(IConfiguration configuration) : IEmailTransport
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var host = configuration[EmailSettings.SmtpHostKey]
            ?? throw new InvalidOperationException("Email:Smtp:Host must be configured.");
        using var client = new SmtpClient(host, configuration.GetValue("Email:Smtp:Port", 587))
        {
            // Relays used in production require STARTTLS; it can only be disabled for a local test relay.
            EnableSsl = configuration.GetValue("Email:Smtp:EnableSsl", true),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15_000
        };
        var username = configuration["Email:Smtp:Username"];
        if (!string.IsNullOrEmpty(username))
            client.Credentials = new NetworkCredential(username, configuration["Email:Smtp:Password"]);

        using var mail = new MailMessage
        {
            From = new MailAddress(EmailSettings.FromAddress(configuration), EmailSettings.FromName(configuration)),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = false,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        mail.To.Add(new MailAddress(message.ToEmail, message.ToName));
        await client.SendMailAsync(mail, cancellationToken);
    }
}
