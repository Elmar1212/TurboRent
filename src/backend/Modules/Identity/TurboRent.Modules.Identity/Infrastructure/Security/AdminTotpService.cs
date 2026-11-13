using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using TurboRent.Modules.Identity.Domain;

namespace TurboRent.Modules.Identity.Infrastructure.Security;

public sealed class AdminTotpService(
    IDataProtectionProvider protectionProvider,
    UserManager<ApplicationUser> users,
    TimeProvider timeProvider)
{
    private const string Provider = "TurboRent.AdminMfa";
    private const string TokenName = "TotpSecret";
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    private readonly IDataProtector _protector =
        protectionProvider.CreateProtector("TurboRent.AdminMfa.TotpSecret.v1");

    public static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        var result = new System.Text.StringBuilder(32);
        var buffer = 0;
        var bits = 0;
        foreach (var value in bytes)
        {
            buffer = (buffer << 8) | value;
            bits += 8;
            while (bits >= 5)
            {
                bits -= 5;
                result.Append(Alphabet[(buffer >> bits) & 31]);
            }
        }
        if (bits > 0) result.Append(Alphabet[(buffer << (5 - bits)) & 31]);
        CryptographicOperations.ZeroMemory(bytes);
        return result.ToString();
    }

    public async Task<IdentityResult> SetSecretAsync(ApplicationUser user, string secret)
    {
        var encrypted = _protector.Protect(secret);
        var result = await users.SetAuthenticationTokenAsync(user, Provider, TokenName, encrypted);
        if (!result.Succeeded) return result;
        user.TwoFactorEnabled = true;
        return await users.UpdateAsync(user);
    }

    public async Task<bool> VerifyAndConsumeAsync(ApplicationUser user, string? code)
    {
        if (code is not { Length: 6 } || code.Any(character => character is < '0' or > '9') ||
            !user.TwoFactorEnabled)
            return false;

        var encrypted = await users.GetAuthenticationTokenAsync(user, Provider, TokenName);
        if (string.IsNullOrEmpty(encrypted)) return false;

        string secret;
        try { secret = _protector.Unprotect(encrypted); }
        catch (CryptographicException) { return false; }

        byte[] key;
        try { key = DecodeBase32(secret); }
        catch (FormatException) { return false; }

        var currentStep = timeProvider.GetUtcNow().ToUnixTimeSeconds() / 30;
        var acceptedStep = -1L;
        var supplied = System.Text.Encoding.ASCII.GetBytes(code);
        try
        {
            foreach (var step in new[] { currentStep - 1, currentStep, currentStep + 1 })
            {
                if (user.LastAcceptedTotpStep is long lastStep && step <= lastStep) continue;
                var expected = System.Text.Encoding.ASCII.GetBytes(GenerateCode(key, step));
                try
                {
                    if (CryptographicOperations.FixedTimeEquals(supplied, expected))
                        acceptedStep = Math.Max(acceptedStep, step);
                }
                finally { CryptographicOperations.ZeroMemory(expected); }
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(supplied);
            CryptographicOperations.ZeroMemory(key);
        }

        if (acceptedStep < 0) return false;
        user.LastAcceptedTotpStep = acceptedStep;
        var update = await users.UpdateAsync(user);
        return update.Succeeded;
    }

    private static string GenerateCode(byte[] key, long step)
    {
        Span<byte> counter = stackalloc byte[sizeof(long)];
        System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(counter, step);
        var hash = HMACSHA1.HashData(key, counter);
        try
        {
            var offset = hash[^1] & 0x0f;
            var binary = ((hash[offset] & 0x7f) << 24) |
                         (hash[offset + 1] << 16) |
                         (hash[offset + 2] << 8) |
                         hash[offset + 3];
            return (binary % 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        }
        finally { CryptographicOperations.ZeroMemory(hash); }
    }

    private static byte[] DecodeBase32(string value)
    {
        if (value.Length != 32 || value.Any(character => !Alphabet.Contains(character)))
            throw new FormatException("Invalid TOTP secret.");
        var result = new byte[value.Length * 5 / 8];
        var buffer = 0;
        var bits = 0;
        var position = 0;
        foreach (var character in value)
        {
            buffer = (buffer << 5) | Alphabet.IndexOf(character);
            bits += 5;
            if (bits < 8) continue;
            bits -= 8;
            result[position++] = (byte)(buffer >> bits);
        }
        return result;
    }
}
