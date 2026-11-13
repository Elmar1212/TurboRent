using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TurboRent.Modules.Identity.Infrastructure.Email;

/// <summary>
/// Development transport: writes each message as JSON and plain text into a local mailbox folder
/// instead of sending it. It refuses to run outside Development and Testing.
/// </summary>
public sealed class LocalMailbox(IConfiguration configuration, IHostEnvironment environment) : IEmailTransport
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (!(environment.IsDevelopment() || environment.IsEnvironment("Testing")))
            throw new InvalidOperationException("Local mailbox is unavailable in this environment.");
        var root = configuration["Identity:MailboxPath"] ?? Path.Combine(LocalIdentityFiles.Root, "mailbox");
        LocalIdentityFiles.ProtectDirectory(root, configuration.GetValue("Identity:MailboxHostMount", false));

        var id = Guid.NewGuid();
        var sentAt = DateTimeOffset.UtcNow;
        var fromName = EmailSettings.FromName(configuration);
        var fromEmail = EmailSettings.FromAddress(configuration);
        var json = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["kind"] = message.Kind,
            ["fromName"] = fromName,
            ["fromEmail"] = fromEmail,
            ["toName"] = message.ToName,
            ["email"] = message.ToEmail,
            ["subject"] = message.Subject,
            ["body"] = message.Body,
            ["locale"] = message.Locale,
            [message.LinkName] = message.LinkUrl,
            ["createdAtUtc"] = sentAt
        };
        var nl = Environment.NewLine;
        var text = $"From: {fromName} <{fromEmail}>{nl}" +
            $"To: {message.ToName} <{message.ToEmail}>{nl}" +
            $"Subject: {message.Subject}{nl}" +
            $"Date: {sentAt:O}{nl}{nl}{message.Body}{nl}";

        // Temporary files are ignored by mailbox readers; atomic rename publishes a complete message.
        var textTemporary = Path.Combine(root, id + ".txt.tmp");
        await File.WriteAllTextAsync(textTemporary, text, cancellationToken);
        File.Move(textTemporary, Path.Combine(root, id + ".txt"));
        var temporary = Path.Combine(root, id + ".tmp");
        await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(json), cancellationToken);
        File.Move(temporary, Path.Combine(root, id + ".json"));
    }
}
