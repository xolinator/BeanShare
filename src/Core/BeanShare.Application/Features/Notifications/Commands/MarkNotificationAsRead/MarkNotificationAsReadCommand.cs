using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(
    Guid NotificationId
) : ICommand<Result<Unit>>;
