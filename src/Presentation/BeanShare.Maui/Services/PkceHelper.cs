using System.Security.Cryptography;
using System.Text;

namespace BeanShare.Maui.Services;

/// <summary>
/// Provides PKCE (Proof Key for Code Exchange) utilities for OAuth 2.0 flows.
/// </summary>
public static class PkceHelper
{
    /// <summary>
    /// Generates a cryptographically random code verifier for PKCE.
    /// </summary>
    public static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// Creates a S256 code challenge from the given code verifier.
    /// </summary>
    public static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// Generates a cryptographically random URL-safe string of the specified byte length.
    /// </summary>
    public static string GenerateRandomString(int length)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
