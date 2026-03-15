using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

public sealed class User
{
    public UserId Id { get; private set; }
    public string Email { get; private set; }
    public string Name { get; private set; }
    public string? PictureUrl { get; private set; }
    public AuthenticationProvider Provider { get; private set; }
    public string? ProviderUserId { get; private set; }
    public string? PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    public string? PreferredCurrencyCode { get; private set; }
    public SystemRole SystemRole { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    private User()
    {
        Email = string.Empty;
        Name = string.Empty;
        SystemRole = SystemRole.User;
        IsActive = true;
    }

    private User(UserId id, string email, string name, AuthenticationProvider provider, DateTime createdAt, string? providerUserId = null, string? passwordHash = null, string? pictureUrl = null)
    {
        Id = id;
        Email = email;
        Name = name;
        Provider = provider;
        ProviderUserId = providerUserId;
        PasswordHash = passwordHash;
        PictureUrl = pictureUrl;
        CreatedAt = createdAt;
        LastLoginAt = createdAt;
        SystemRole = SystemRole.User;
        IsActive = true;
    }

    public static User CreateWithPassword(string email, string name, string passwordHash, DateTime createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(new UserId(Guid.NewGuid()), email, name, AuthenticationProvider.Email, createdAt, null, passwordHash, null);
    }

    public static User CreateWithIdAndPassword(Guid id, string email, string name, string passwordHash, DateTime createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User(new UserId(id), email, name, AuthenticationProvider.Email, createdAt, null, passwordHash, null);
    }

    public static User CreateWithProvider(string email, string name, AuthenticationProvider provider, string providerUserId, DateTime createdAt, string? pictureUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerUserId);

        return new User(new UserId(Guid.NewGuid()), email, name, provider, createdAt, providerUserId, null, pictureUrl);
    }

    public static User CreateFromOidc(Guid oidcUserId, string email, string name, AuthenticationProvider provider, DateTime createdAt, string? pictureUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new User(new UserId(oidcUserId), email, name, provider, createdAt, oidcUserId.ToString(), null, pictureUrl);
    }

    public void UpdateLastLogin(DateTime lastLoginAt)
    {
        LastLoginAt = lastLoginAt;
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

    public void SetPreferredCurrency(string? currencyCode)
    {
        if (currencyCode is not null)
        {
            _ = Currency.Create(currencyCode);
        }
        PreferredCurrencyCode = currencyCode?.ToUpperInvariant();
    }

    public void PromoteToSystemAdmin()
    {
        SystemRole = SystemRole.SystemAdmin;
    }

    public void DemoteToUser()
    {
        SystemRole = SystemRole.User;
    }

    public void SetSystemRole(SystemRole role)
    {
        SystemRole = role;
    }

    public void Deactivate(DateTime deactivatedAt)
    {
        if (!IsActive)
            return;

        IsActive = false;
        DeactivatedAt = deactivatedAt;
    }

    public void Reactivate()
    {
        if (IsActive)
            return;

        IsActive = true;
        DeactivatedAt = null;
    }

    public bool IsSystemAdmin => SystemRole == SystemRole.SystemAdmin;
}
