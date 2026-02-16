using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Queries.GetUserNotifications;
public sealed class GetUserNotificationsHandler : IRequestHandler<GetUserNotificationsQuery, Result<NotificationListDto>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;

    public GetUserNotificationsHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
    }

    public async Task<Result<NotificationListDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.CurrentUserId;

        var notifications = request.UnreadOnly
            ? await _notificationRepository.GetUnreadByUserIdAsync(userId, cancellationToken)
            : await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);

        var unreadCount = await _notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto(
            n.Id.Value,
            n.Type.ToString(),
            n.Title,
            n.Message,
            n.SpaceId?.Value,
            n.IsRead,
            n.CreatedAt,
            n.ReadAt,
            n.ActionUrl
        )).ToList();

        return Result<NotificationListDto>.Success(new NotificationListDto(
            notifications.Count,
            unreadCount,
            dtos
        ));
    }
}
