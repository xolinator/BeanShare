using BeanShare.Contracts.Notifications;

namespace BeanShare.Maui.Services;

public interface INotificationService
{
    Task<NotificationListResponse?> GetNotificationsAsync(bool unreadOnly = false);
    Task<int> GetUnreadCountAsync();
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<int> MarkAllAsReadAsync();
    Task<int> ClearAllAsync();
}
