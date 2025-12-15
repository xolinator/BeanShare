namespace BeanShare.Contracts.Notifications;

public sealed record GetNotificationsRequest(
    bool UnreadOnly = false
);

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Message,
    Guid? SpaceId,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt,
    string? ActionUrl
);

public sealed record NotificationListResponse(
    int TotalCount,
    int UnreadCount,
    IReadOnlyList<NotificationResponse> Notifications
);

public sealed record UnreadCountResponse(
    int Count
);

public sealed record MarkNotificationAsReadRequest(
    Guid NotificationId
);

public sealed record MarkAsReadResponse(
    int MarkedCount
);

public sealed record ClearNotificationsResponse(
    int DeletedCount
);
