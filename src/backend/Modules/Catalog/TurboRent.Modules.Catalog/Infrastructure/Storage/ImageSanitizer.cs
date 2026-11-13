using System.Buffers.Binary;

namespace TurboRent.Modules.Catalog.Infrastructure.Storage;

/// <summary>
/// Detects JPEG, PNG and WebP images and removes embedded metadata (EXIF including GPS location,
/// XMP, IPTC, text comments) before an image is stored and shown publicly. Pixel data and colour
/// information are kept byte for byte; no re-encoding takes place. Malformed files are rejected.
/// </summary>
/// <remarks>
/// Removing EXIF also removes the orientation tag. The web client re-encodes photos in the
/// browser (which applies orientation) before upload, so this only affects direct API uploads.
/// </remarks>
public static class ImageSanitizer
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a];

    /// <summary>PNG chunks needed to render the image correctly; everything else is dropped.</summary>
    private static readonly HashSet<string> PngKeep =
        ["IHDR", "PLTE", "IDAT", "IEND", "tRNS", "gAMA", "cHRM", "sRGB", "iCCP", "sBIT", "pHYs", "bKGD", "acTL", "fcTL", "fdAT"];

    public static string? DetectContentType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith(PngSignature)) return "image/png";
        if (bytes.Length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff) return "image/jpeg";
        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
    }

    /// <summary>Returns the image without metadata, or null if the data is not a well-formed image.</summary>
    public static byte[]? Sanitize(byte[] data, string contentType) => contentType switch
    {
        "image/jpeg" => SanitizeJpeg(data),
        "image/png" => SanitizePng(data),
        "image/webp" => SanitizeWebp(data),
        _ => null
    };

    private static byte[]? SanitizeJpeg(byte[] data)
    {
        if (data.Length < 4 || data[0] != 0xff || data[1] != 0xd8) return null;
        using var output = new MemoryStream(data.Length);
        output.Write(data, 0, 2);
        var position = 2;
        while (position + 4 <= data.Length)
        {
            if (data[position] != 0xff) return null;
            var marker = data[position + 1];
            if (marker == 0xff) { position++; continue; } // fill byte
            if (marker is >= 0xd0 and <= 0xd7 or 0x01)    // markers without a length
            {
                output.Write(data, position, 2);
                position += 2;
                continue;
            }
            var length = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(position + 2));
            if (length < 2 || position + 2 + length > data.Length) return null;
            if (marker == 0xda) // start of scan: entropy-coded data follows until the end
            {
                output.Write(data, position, data.Length - position);
                return output.ToArray();
            }
            // APP1 = EXIF/XMP, APP13 = IPTC/Photoshop, COM = comment. APP0, APP2 (ICC) and APP14 stay.
            var drop = marker is 0xe1 or 0xed or 0xfe;
            if (!drop) output.Write(data, position, 2 + length);
            position += 2 + length;
        }
        return null; // no image data found
    }

    private static byte[]? SanitizePng(byte[] data)
    {
        if (data.Length < 8 || !data.AsSpan(0, 8).SequenceEqual(PngSignature)) return null;
        using var output = new MemoryStream(data.Length);
        output.Write(PngSignature);
        var position = 8;
        var sawHeader = false;
        var sawData = false;
        while (position + 12 <= data.Length)
        {
            var length = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(position));
            if (length > int.MaxValue || position + 12L + length > data.Length) return null;
            var type = System.Text.Encoding.ASCII.GetString(data, position + 4, 4);
            if (!sawHeader && type != "IHDR") return null;
            sawHeader = true;
            sawData |= type == "IDAT";
            var total = 12 + (int)length;
            if (type == "IEND")
            {
                // Canonical end marker; anything appended after the image is dropped.
                output.Write(PngEnd);
                return output.ToArray();
            }
            if (PngKeep.Contains(type)) output.Write(data, position, total);
            position += total;
        }
        // Browsers render PNGs whose end marker is missing or cut short; store them with a proper one.
        if (!sawData || position < data.Length - 12) return null;
        output.Write(PngEnd);
        return output.ToArray();
    }

    private static readonly byte[] PngEnd = [0, 0, 0, 0, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82];

    private static byte[]? SanitizeWebp(byte[] data)
    {
        if (data.Length < 20 || DetectContentType(data) != "image/webp") return null;
        var riffEnd = 8L + BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(4));
        if (riffEnd > data.Length) return null;
        using var body = new MemoryStream(data.Length);
        body.Write("WEBP"u8);
        var position = 12;
        var vp8xFlagsOffset = -1;
        while (position + 8 <= riffEnd)
        {
            var size = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(position + 4));
            var padded = size + (size & 1);
            if (position + 8L + padded > riffEnd) return null;
            var fourCc = System.Text.Encoding.ASCII.GetString(data, position, 4);
            if (fourCc is not ("EXIF" or "XMP "))
            {
                if (fourCc == "VP8X" && size >= 1) vp8xFlagsOffset = (int)body.Position + 8;
                body.Write(data, position, 8 + (int)padded);
            }
            position += 8 + (int)padded;
        }
        if (position != riffEnd) return null;
        var bytes = body.ToArray();
        // Clear the "has EXIF" (0x08) and "has XMP" (0x04) flags now that those chunks are gone.
        if (vp8xFlagsOffset >= 0) bytes[vp8xFlagsOffset] &= unchecked((byte)~0x0c);
        var result = new byte[8 + bytes.Length];
        "RIFF"u8.CopyTo(result);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4), (uint)bytes.Length);
        bytes.CopyTo(result, 8);
        return result;
    }
}
