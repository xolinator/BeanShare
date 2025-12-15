using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Queries.GetUserNotifications;

public sealed record GetUserNotificationsQuery(
    bool UnreadOnly = false
) : IRequest<Result<NotificationListDto>>;
