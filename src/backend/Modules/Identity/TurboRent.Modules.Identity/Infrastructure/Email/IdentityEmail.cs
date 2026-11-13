using System.Net.Mail;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TurboRent.Modules.Identity.Contracts;
using TurboRent.Modules.Identity.Domain;
using TurboRent.Modules.Identity.Infrastructure.Persistence;

namespace TurboRent.Modules.Identity.Infrastructure.Email;

public interface IIdentityEmail
{
    Task SendVerificationAsync(ApplicationUser user, string identityToken, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(ApplicationUser user, string identityToken, CancellationToken cancellationToken);
}

/// <summary>Composes localized account and booking emails and hands them to the configured transport.</summary>
public sealed class IdentityEmail(
    IEmailTransport transport,
    IConfiguration configuration,
    IHostEnvironment environment,
    IdentityDbContext db,
    ILogger<IdentityEmail> logger) : IIdentityEmail, IUserNotifier
{
    /// <summary>Exceptions that mean "could not deliver right now" rather than a programming error.</summary>
    public static bool IsDeliveryFailure(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or SmtpException;

    public Task SendVerificationAsync(ApplicationUser user, string identityToken, CancellationToken cancellationToken)
    {
        var url = TokenLink("/verify-email", user.Id, identityToken);
        var en = user.Locale == "en";
        var nl = Environment.NewLine;
        var body = en
            ? $"Hello {user.DisplayName},{nl}{nl}Confirm your email address to activate your TurboRent account:{nl}{nl}{url}{nl}{nl}This link expires in 24 hours."
            : $"Salam {user.DisplayName},{nl}{nl}TurboRent hesabınızı aktivləşdirmək üçün email ünvanınızı təsdiqləyin:{nl}{nl}{url}{nl}{nl}Bu link 24 saat ərzində etibarlıdır.";
        return transport.SendAsync(new EmailMessage(user.DisplayName, user.Email!,
            en ? "TurboRent - Verify your email" : "TurboRent - Emailinizi təsdiqləyin",
            body, user.Locale, "verification", "verificationUrl", url), cancellationToken);
    }

    public Task SendPasswordResetAsync(ApplicationUser user, string identityToken, CancellationToken cancellationToken)
    {
        var url = TokenLink("/reset-password", user.Id, identityToken);
        var en = user.Locale == "en";
        var nl = Environment.NewLine;
        var body = en
            ? $"Hello {user.DisplayName},{nl}{nl}We received a request to reset your TurboRent password. Choose a new password here:{nl}{nl}{url}{nl}{nl}This link expires in 2 hours. If you did not ask for this, you can ignore this email; your password stays the same."
            : $"Salam {user.DisplayName},{nl}{nl}TurboRent parolunuzu sıfırlamaq üçün sorğu aldıq. Yeni parolu buradan seçin:{nl}{nl}{url}{nl}{nl}Bu link 2 saat ərzində etibarlıdır. Sorğunu siz göndərməmisinizsə, bu məktubu nəzərə almayın; parolunuz dəyişməyəcək.";
        return transport.SendAsync(new EmailMessage(user.DisplayName, user.Email!,
            en ? "TurboRent - Reset your password" : "TurboRent - Parolun sıfırlanması",
            body, user.Locale, "password_reset", "resetUrl", url), cancellationToken);
    }

    public async Task NotifyAsync(Guid userId, UserNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            var user = await db.Users.AsNoTracking()
                .Where(item => item.Id == userId && item.EmailConfirmed && item.AccountStatus == "Active")
                .Select(item => new { item.Email, item.DisplayName, item.Locale })
                .SingleOrDefaultAsync(cancellationToken);
            if (user?.Email is null) return;
            var en = user.Locale == "en";
            var (subject, intro) = Texts(notification.Kind, en);
            var url = Origin().TrimEnd('/') + notification.LinkPath;
            var nl = Environment.NewLine;
            var details = string.IsNullOrWhiteSpace(notification.Details) ? "" : notification.Details + nl + nl;
            var body = (en ? $"Hello {user.DisplayName},{nl}{nl}" : $"Salam {user.DisplayName},{nl}{nl}") +
                $"{intro}{nl}{nl}{notification.Subject}{nl}{details}{url}{nl}";
            await transport.SendAsync(new EmailMessage(user.DisplayName, user.Email,
                $"TurboRent - {subject}", body, user.Locale, notification.Kind.ToString(), "linkUrl", url),
                cancellationToken);
        }
        catch (Exception exception) when (IsDeliveryFailure(exception) || exception is InvalidOperationException)
        {
            // Never log addresses or message content. The action itself already succeeded.
            logger.LogWarning("Notification delivery failed. Kind: {Kind}. FailureType: {FailureType}",
                notification.Kind, exception.GetType().Name);
        }
    }

    private static (string Subject, string Intro) Texts(UserNotificationKind kind, bool en) => kind switch
    {
        UserNotificationKind.BookingRequested => en
            ? ("New booking request", "A renter requested one of your cars. Confirm or decline the request:")
            : ("Yeni rezerv sorğusu", "Avtomobiliniz üçün yeni rezerv sorğusu var. Sorğunu təsdiqləyin və ya rədd edin:"),
        UserNotificationKind.BookingConfirmed => en
            ? ("Booking confirmed", "Good news: the owner confirmed your booking.")
            : ("Rezerv təsdiqləndi", "Sahib rezervinizi təsdiqlədi."),
        UserNotificationKind.BookingDeclined => en
            ? ("Booking declined", "Unfortunately the owner declined your booking request.")
            : ("Rezerv rədd edildi", "Təəssüf ki, sahib rezerv sorğunuzu rədd etdi."),
        UserNotificationKind.BookingCancelled => en
            ? ("Booking cancelled", "A booking for your car was cancelled by the renter.")
            : ("Rezerv ləğv edildi", "Avtomobiliniz üçün rezerv icarəçi tərəfindən ləğv edildi."),
        UserNotificationKind.ListingApproved => en
            ? ("Listing approved", "Your listing was approved and is now visible in the catalog.")
            : ("Elan təsdiqləndi", "Elanınız təsdiqləndi və kataloqda görünür."),
        UserNotificationKind.ListingRejected => en
            ? ("Listing rejected", "Your listing was not approved. Review the reason, edit the listing and submit it again.")
            : ("Elan rədd edildi", "Elanınız təsdiqlənmədi. Səbəbə baxın, elanı düzəldin və yenidən göndərin."),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private string TokenLink(string path, Guid userId, string identityToken)
    {
        var token = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(identityToken));
        // Tokens travel in the fragment so they never reach server logs or the Referer header.
        return Origin().TrimEnd('/') + path + "#userId=" + userId + "&token=" + token;
    }

    private string Origin()
    {
        var local = environment.IsDevelopment() || environment.IsEnvironment("Testing");
        var origin = configuration["Identity:FrontendOrigin"] ?? (local ? "https://127.0.0.1:5174" : null);
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            throw new InvalidOperationException("Identity:FrontendOrigin must be an absolute HTTPS URL.");
        if (local && transport is LocalMailbox && !uri.IsLoopback)
            throw new InvalidOperationException("Local mailbox requires an HTTPS loopback frontend.");
        return origin!;
    }
}
