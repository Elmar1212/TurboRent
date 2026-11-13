using System.Buffers.Binary;
using System.Text;
using TurboRent.Modules.Catalog.Domain;
using TurboRent.Modules.Catalog.Features.Shared;
using TurboRent.Modules.Catalog.Infrastructure.Storage;

namespace TurboRent.Identity.UnitTests;

public class ImageSanitizerTests
{
    // 1x1 PNG whose IEND chunk is one byte short, as some encoders produce.
    private static readonly byte[] TruncatedPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jWQAAAABJRU5ErkJggg==");
    private static readonly byte[] IendChunk = [0, 0, 0, 0, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82];
    private static readonly byte[] Png = [.. TruncatedPng.AsSpan(0, 56), .. IendChunk];

    [Fact]
    public void Png_with_a_cut_off_end_marker_is_repaired() =>
        Assert.Equal(Png, ImageSanitizer.Sanitize(TruncatedPng, "image/png"));

    private static byte[] Chunk(string type, byte[] data)
    {
        var chunk = new byte[12 + data.Length];
        BinaryPrimitives.WriteUInt32BigEndian(chunk, (uint)data.Length);
        Encoding.ASCII.GetBytes(type).CopyTo(chunk, 4);
        data.CopyTo(chunk, 8);
        return chunk; // CRC is not checked by the sanitizer
    }

    [Fact]
    public void Jpeg_exif_and_comments_are_removed_but_image_data_is_kept()
    {
        byte[] exif = [.. "Exif\0\0"u8, .. "GPS 40.40,49.86"u8];
        byte[] jpeg =
        [
            0xff, 0xd8,
            0xff, 0xe0, 0x00, 0x04, 0x4a, 0x46,                                 // APP0 (kept)
            0xff, 0xe1, 0x00, (byte)(exif.Length + 2), .. exif,                  // APP1 EXIF (removed)
            0xff, 0xfe, 0x00, 0x06, (byte)'h', (byte)'i', (byte)'!', (byte)'!',  // comment (removed)
            0xff, 0xda, 0x00, 0x04, 0x01, 0x02, 0xaa, 0xbb, 0xff, 0xd9           // scan data (kept)
        ];
        var clean = ImageSanitizer.Sanitize(jpeg, "image/jpeg");
        Assert.NotNull(clean);
        Assert.Equal(new byte[]
        {
            0xff, 0xd8, 0xff, 0xe0, 0x00, 0x04, 0x4a, 0x46,
            0xff, 0xda, 0x00, 0x04, 0x01, 0x02, 0xaa, 0xbb, 0xff, 0xd9
        }, clean);
    }

    [Fact]
    public void Png_text_chunks_and_trailing_data_are_removed()
    {
        var iend = Png.Length - 12;
        byte[] withMetadata =
        [
            .. Png.AsSpan(0, iend),
            .. Chunk("tEXt", "Location\0Baku"u8.ToArray()),
            .. Chunk("eXIf", [1, 2, 3]),
            .. Png.AsSpan(iend),
            .. "appended payload"u8
        ];
        Assert.Equal(Png, ImageSanitizer.Sanitize(withMetadata, "image/png"));
    }

    [Fact]
    public void Webp_exif_chunk_is_removed_and_header_flags_are_cleared()
    {
        byte[] vp8x = [0x0c, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        byte[] Body(string fourCc, byte[] data)
        {
            var chunk = new byte[8 + data.Length + (data.Length & 1)];
            Encoding.ASCII.GetBytes(fourCc).CopyTo(chunk, 0);
            BinaryPrimitives.WriteUInt32LittleEndian(chunk.AsSpan(4), (uint)data.Length);
            data.CopyTo(chunk, 8);
            return chunk;
        }
        byte[] chunks = [.. "WEBP"u8, .. Body("VP8X", vp8x), .. Body("VP8L", [1, 2, 3, 4]), .. Body("EXIF", [9, 9, 9])];
        byte[] webp = [.. "RIFF"u8, 0, 0, 0, 0, .. chunks];
        BinaryPrimitives.WriteUInt32LittleEndian(webp.AsSpan(4), (uint)chunks.Length);

        var clean = ImageSanitizer.Sanitize(webp, "image/webp");
        Assert.NotNull(clean);
        Assert.Equal("image/webp", ImageSanitizer.DetectContentType(clean));
        Assert.DoesNotContain("EXIF", Encoding.ASCII.GetString(clean));
        Assert.Equal(0, clean[20] & 0x0c); // first VP8X payload byte
        Assert.Equal((uint)(clean.Length - 8), BinaryPrimitives.ReadUInt32LittleEndian(clean.AsSpan(4)));
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void Malformed_images_are_rejected(string contentType)
    {
        var truncated = contentType switch
        {
            "image/png" => Png.AsSpan(0, 20).ToArray(),
            "image/jpeg" => new byte[] { 0xff, 0xd8, 0xff, 0xe0, 0x00, 0x40 },
            _ => [.. "RIFF"u8, 0xff, 0, 0, 0, .. "WEBP"u8, 1, 2, 3, 4, 5, 6, 7, 8]
        };
        Assert.Null(ImageSanitizer.Sanitize(truncated, contentType));
    }

    [Fact]
    public void Non_images_are_not_detected() =>
        Assert.Null(ImageSanitizer.DetectContentType("<svg onload=alert(1)>"u8));
}

public class BookingTests
{
    private static readonly DateOnly Start = new(2026, 12, 1);

    [Fact]
    public void Total_price_is_daily_price_times_inclusive_days()
    {
        var booking = Booking.Request(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Start, Start.AddDays(2),
            75.5m, "  hi  ", DateTimeOffset.UtcNow);
        Assert.Equal(3, booking.Days);
        Assert.Equal(226.5m, booking.TotalPrice);
        Assert.Equal("hi", booking.Message);
        Assert.Equal(BookingStatus.Requested, booking.Status);
    }

    [Fact]
    public void Owners_cannot_book_their_own_car_and_long_or_reversed_ranges_fail()
    {
        var owner = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() =>
            Booking.Request(Guid.NewGuid(), owner, owner, Start, Start, 50, null, DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() =>
            Booking.Request(Guid.NewGuid(), Guid.NewGuid(), owner, Start, Start.AddDays(-1), 50, null, DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() =>
            Booking.Request(Guid.NewGuid(), Guid.NewGuid(), owner, Start, Start.AddDays(Booking.MaxDays), 50, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Status_changes_follow_the_lifecycle()
    {
        var booking = Booking.Request(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Start, Start, 50, null, DateTimeOffset.UtcNow);
        var version = booking.Version;
        booking.Confirm(DateTimeOffset.UtcNow);
        Assert.NotEqual(version, booking.Version);
        Assert.Throws<InvalidOperationException>(() => booking.Decline(DateTimeOffset.UtcNow));
        booking.Cancel(DateTimeOffset.UtcNow);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Throws<InvalidOperationException>(() => booking.Cancel(DateTimeOffset.UtcNow));
    }

    [Theory]
    [InlineData(0, 0, true)]   // same day
    [InlineData(2, 4, true)]   // touching the last day
    [InlineData(3, 5, false)]  // starts the day after
    [InlineData(-3, -1, false)]
    public void Overlap_uses_inclusive_days(int fromOffset, int toOffset, bool expected)
    {
        var booking = Booking.Request(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Start, Start.AddDays(2), 50, null, DateTimeOffset.UtcNow);
        Assert.Equal(expected, booking.Overlaps(Start.AddDays(fromOffset), Start.AddDays(toOffset)));
    }
}

public class ListingRulesTests
{
    [Fact]
    public void Valid_input_uses_the_azerbaijani_description_when_english_is_blank()
    {
        var draft = ListingRules.Validate(1, 2022, "baku", 80.5m, "automatic", "hybrid", 5, " Təsvir ", "  ");
        Assert.NotNull(draft);
        Assert.Equal("Təsvir", draft.DescriptionAz);
        Assert.Equal("Təsvir", draft.DescriptionEn);
    }

    [Theory]
    [InlineData(0, 2022, "baku", 80, "automatic", "petrol", 5)]
    [InlineData(1, 1979, "baku", 80, "automatic", "petrol", 5)]
    [InlineData(1, 2022, "paris", 80, "automatic", "petrol", 5)]
    [InlineData(1, 2022, "baku", 0, "automatic", "petrol", 5)]
    [InlineData(1, 2022, "baku", 80, "cvt", "petrol", 5)]
    [InlineData(1, 2022, "baku", 80, "automatic", "coal", 5)]
    [InlineData(1, 2022, "baku", 80, "automatic", "petrol", 12)]
    public void Invalid_fields_are_rejected(int modelId, int year, string city, decimal price,
        string transmission, string fuel, int seats) =>
        Assert.Null(ListingRules.Validate(modelId, year, city, price, transmission, fuel, seats, "Təsvir", null));

    [Fact]
    public void Prices_with_more_than_two_decimals_are_rejected() =>
        Assert.Null(ListingRules.Validate(1, 2022, "baku", 80.123m, "automatic", "petrol", 5, "Təsvir", null));
}
