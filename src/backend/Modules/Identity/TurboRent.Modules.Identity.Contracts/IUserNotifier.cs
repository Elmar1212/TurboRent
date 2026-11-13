namespace TurboRent.Modules.Identity.Contracts;

/// <summary>Sends transactional notifications to a user's verified email address.</summary>
public interface IUserNotifier
{
    /// <summary>Best effort: delivery failures are logged and never thrown to the caller.</summary>
    Task NotifyAsync(Guid userId, UserNotification notification, CancellationToken cancellationToken);
}

public enum UserNotificationKind
{
    BookingRequested,
    BookingConfirmed,
    BookingDeclined,
    BookingCancelled,
    ListingApproved,
    ListingRejected
}

/// <param name="Subject">Short context such as the vehicle name, never personal data of another user.</param>
/// <param name="LinkPath">Site-relative path, for example <c>/account/bookings</c>.</param>
public sealed record UserNotification(UserNotificationKind Kind, string Subject, string? Details, string LinkPath);
