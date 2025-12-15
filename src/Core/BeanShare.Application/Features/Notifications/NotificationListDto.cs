namespace BeanShare.Application.Features.Notifications;

public sealed record NotificationListDto(
    int TotalCount,
    int UnreadCount,
    IReadOnlyList<NotificationDto> Notifications
);
