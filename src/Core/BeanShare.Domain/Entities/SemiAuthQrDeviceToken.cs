using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

public sealed class SemiAuthQrDeviceToken : Entity
{
    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public string DeviceIdHash { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevocationReason { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public SpaceId? LastUsedSpaceId { get; private set; }
    public ActiveQrCodeId? LastUsedQrCodeId { get; private set; }

    private SemiAuthQrDeviceToken()
    {
        DeviceIdHash = string.Empty;
        TokenHash = string.Empty;
        UserId = default;
    }

    private SemiAuthQrDeviceToken(
        Guid id,
        UserId userId,
        string deviceIdHash,
        string tokenHash,
        DateTime createdAt,
        DateTime expiresAt)
    {
        Id = id;
        UserId = userId;
        DeviceIdHash = deviceIdHash;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public static SemiAuthQrDeviceToken Create(
        UserId userId,
        string deviceIdHash,
        string tokenHash,
        DateTime expiresAt,
        IClock clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceIdHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        ArgumentNullException.ThrowIfNull(clock);

        return new SemiAuthQrDeviceToken(
            Guid.NewGuid(),
            userId,
            deviceIdHash.Trim(),
            tokenHash.Trim(),
            clock.UtcNow,
            expiresAt);
    }

    public void Rotate(string tokenHash, DateTime expiresAt, IClock clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        ArgumentNullException.ThrowIfNull(clock);

        TokenHash = tokenHash.Trim();
        ExpiresAt = expiresAt;
        IsRevoked = false;
        RevokedAt = null;
        RevocationReason = null;
        UpdatedAt = clock.UtcNow;
    }

    public void Revoke(string reason, IClock clock)
    {
        if (IsRevoked)
            return;

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        ArgumentNullException.ThrowIfNull(clock);

        IsRevoked = true;
        RevocationReason = reason.Trim();
        RevokedAt = clock.UtcNow;
        UpdatedAt = clock.UtcNow;
    }

    public bool CanBeUsed(DateTime now) => !IsRevoked && now <= ExpiresAt;

    public void MarkUsed(DateTime usedAt, SpaceId spaceId, ActiveQrCodeId qrCodeId)
    {
        LastUsedAt = usedAt;
        LastUsedSpaceId = spaceId;
        LastUsedQrCodeId = qrCodeId;
        UpdatedAt = usedAt;
    }

    protected override object GetId() => Id;
}
