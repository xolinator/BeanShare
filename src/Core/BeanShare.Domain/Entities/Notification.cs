using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

public sealed class Notification : Entity
{
    public NotificationId Id { get; private set; }
    public UserId UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public SpaceId? SpaceId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? MetadataJson { get; private set; }

    private Notification()
    {
        Id = default;
        UserId = default;
        Title = string.Empty;
        Message = string.Empty;
    }

    private Notification(
        NotificationId id,
        UserId userId,
        NotificationType type,
        string title,
        string message,
        DateTime createdAt,
        SpaceId? spaceId = null,
        string? actionUrl = null,
        string? metadataJson = null)
    {
        Id = id;
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        SpaceId = spaceId;
        IsRead = false;
        CreatedAt = createdAt;
        ActionUrl = actionUrl;
        MetadataJson = metadataJson;
    }

    public static Notification Create(
        UserId userId,
        NotificationType type,
        string title,
        string message,
        IClock clock,
        SpaceId? spaceId = null,
        string? actionUrl = null,
        string? metadataJson = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(clock);

        return new Notification(
            NotificationId.New(),
            userId,
            type,
            title,
            message,
            clock.UtcNow,
            spaceId,
            actionUrl,
            metadataJson);
    }

    public void MarkAsRead(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (!IsRead)
        {
            IsRead = true;
            ReadAt = clock.UtcNow;
        }
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }

    protected override object GetId() => Id;
}
