namespace BeanShare.Infrastructure.Identity;

/// <summary>
/// Service for hashing and verifying passwords using BCrypt
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);
}

/// <summary>
/// Implementation of IPasswordHasher using BCrypt
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        // BCrypt automatically generates a salt and includes it in the hash
        // Work factor of 12 provides good security/performance balance
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword, nameof(hashedPassword));

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}
