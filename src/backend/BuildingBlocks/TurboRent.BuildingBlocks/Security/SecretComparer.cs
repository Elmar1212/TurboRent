using System.Security.Cryptography;
using System.Text;

namespace TurboRent.BuildingBlocks.Security;

public static class SecretComparer
{
    /// <summary>Compares a supplied secret in constant time and wipes the temporary buffers.</summary>
    public static bool FixedTimeEquals(string? supplied, string expected)
    {
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied ?? "");
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        try
        {
            return suppliedBytes.Length == expectedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(suppliedBytes);
            CryptographicOperations.ZeroMemory(expectedBytes);
        }
    }
}
