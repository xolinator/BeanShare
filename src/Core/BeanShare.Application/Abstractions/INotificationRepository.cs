using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;

namespace BeanShare.Application.Abstractions;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default);
    Task DeleteAllByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadByUserIdAsync(UserId userId, IClock clock, CancellationToken cancellationToken = default);
}
