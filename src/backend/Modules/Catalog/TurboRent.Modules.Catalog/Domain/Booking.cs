namespace TurboRent.Modules.Catalog.Domain;

/// <summary>A renter's request to rent a listing for an inclusive range of calendar days.</summary>
public sealed class Booking
{
    public const int MaxDays = 60;
    public const int MaxMessageLength = 500;

    private Booking() { }

    public Guid Id { get; private set; }
    public Guid ListingId { get; private set; }
    public Guid RenterUserId { get; private set; }
    /// <summary>Copied from the listing so owners can query their bookings without a join.</summary>
    public Guid OwnerUserId { get; private set; }
    public DateOnly StartDate { get; private set; }
    /// <summary>Last rental day, inclusive.</summary>
    public DateOnly EndDate { get; private set; }
    /// <summary>Price snapshot at request time; later listing price changes do not affect it.</summary>
    public decimal DailyPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string Status { get; private set; } = BookingStatus.Requested;
    public string? Message { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public Guid Version { get; private set; }

    public int Days => EndDate.DayNumber - StartDate.DayNumber + 1;

    public static Booking Request(Guid listingId, Guid renter, Guid owner, DateOnly start, DateOnly end,
        decimal dailyPrice, string? message, DateTimeOffset now)
    {
        if (renter == owner) throw new InvalidOperationException("Owners cannot book their own listing.");
        if (end < start) throw new ArgumentException("The end date must not be before the start date.");
        var days = end.DayNumber - start.DayNumber + 1;
        if (days > MaxDays) throw new ArgumentException("Bookings are limited to 60 days.");
        return new Booking
        {
            Id = Guid.NewGuid(), ListingId = listingId, RenterUserId = renter, OwnerUserId = owner,
            StartDate = start, EndDate = end, DailyPrice = dailyPrice, TotalPrice = dailyPrice * days,
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
            CreatedAtUtc = now.ToUniversalTime(), Version = Guid.NewGuid()
        };
    }

    public bool Overlaps(DateOnly start, DateOnly end) => StartDate <= end && EndDate >= start;

    public void Confirm(DateTimeOffset now) => Decide(BookingStatus.Confirmed, now);
    public void Decline(DateTimeOffset now) => Decide(BookingStatus.Declined, now);

    public void Cancel(DateTimeOffset now)
    {
        if (Status is not (BookingStatus.Requested or BookingStatus.Confirmed))
            throw new InvalidOperationException("Only open bookings can be cancelled.");
        Status = BookingStatus.Cancelled;
        DecidedAtUtc = now.ToUniversalTime();
        Version = Guid.NewGuid();
    }

    private void Decide(string next, DateTimeOffset now)
    {
        if (Status != BookingStatus.Requested)
            throw new InvalidOperationException("Only requested bookings can be confirmed or declined.");
        Status = next;
        DecidedAtUtc = now.ToUniversalTime();
        Version = Guid.NewGuid();
    }
}

public static class BookingStatus
{
    public const string Requested = "Requested";
    public const string Confirmed = "Confirmed";
    public const string Declined = "Declined";
    public const string Cancelled = "Cancelled";
}
