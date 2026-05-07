using System.Security.Cryptography;
using System.Text;

namespace BeanShare.Application.Features.SemiAuthQr;

internal static class SemiAuthQrCrypto
{
    public static string Hash(string value)
    {
        var trimmed = value.Trim();
        var bytes = Encoding.UTF8.GetBytes(trimmed);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string CreateRawToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        var base64 = Convert.ToBase64String(bytes);
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static bool SecureEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        if (leftBytes.Length != rightBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
