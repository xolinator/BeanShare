using BeanShare.Domain.Common;

namespace BeanShare.Domain.Entities;

/// <summary>
/// Represents an authenticated user in the system
/// </summary>
public sealed class User
{
    public UserId Id { get; private set; }
    public string Email { get; private set; }
    public string Name { get; private set; }
    public string? PictureUrl { get; private set; }
    public string Provider { get; private set; } // "Email", "Google" or "Facebook"
    public string? ProviderUserId { get; private set; } // External provider's user ID (null for email/password)
    public string? PasswordHash { get; private set; } // For email/password authentication
    public DateTime CreatedAt { get; private set; }
    public DateTime LastLoginAt { get; private set; }

    // EF Core constructor
    private User()
    {
        Email = string.Empty;
        Name = string.Empty;
        Provider = string.Empty;
    }

    private User(UserId id, string email, string name, string provider, string? providerUserId = null, string? passwordHash = null, string? pictureUrl = null)
    {
        Id = id;
        Email = email;
        Name = name;
        Provider = provider;
        ProviderUserId = providerUserId;
        PasswordHash = passwordHash;
        PictureUrl = pictureUrl;
        CreatedAt = DateTime.UtcNow;
        LastLoginAt = DateTime.UtcNow;
    }

    public static User CreateWithPassword(string email, string name, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(new UserId(Guid.NewGuid()), email, name, "Email", null, passwordHash, null);
    }

    public static User CreateWithIdAndPassword(Guid id, string email, string name, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(new UserId(id), email, name, "Email", null, passwordHash, null);
    }

    public static User CreateWithProvider(string email, string name, string provider, string providerUserId, string? pictureUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerUserId);

        return new User(new UserId(Guid.NewGuid()), email, name, provider, providerUserId, null, pictureUrl);
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string name, string? pictureUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        PictureUrl = pictureUrl;
    }

    public void UpdatePassword(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    public bool VerifyPassword(string passwordHash)
    {
        return PasswordHash == passwordHash;
    }
}
