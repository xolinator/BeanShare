using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadHandler(
    INotificationRepository notificationRepository,
    IUserContext userContext,
    IClock clock,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkNotificationAsReadCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(
            new NotificationId(request.NotificationId), cancellationToken);

        if (notification == null)
            return Result<Unit>.Failure(Error.NotificationNotFound(request.NotificationId));

        if (notification.UserId != userContext.CurrentUserId)
            return Result<Unit>.Failure(Error.Forbidden("Notification", "You can only mark your own notifications as read"));

        notification.MarkAsRead(clock);
        await notificationRepository.UpdateAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
