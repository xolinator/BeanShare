using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Commands.MarkNotificationAsRead;
public sealed class MarkNotificationAsReadHandler : IRequestHandler<MarkNotificationAsReadCommand, Result<Unit>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext,
        IClock clock,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notificationId = new NotificationId(request.NotificationId);
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        if (notification == null)
        {
            return Result<Unit>.Failure(Error.NotificationNotFound(request.NotificationId));
        }

        if (notification.UserId != _userContext.CurrentUserId)
        {
            return Result<Unit>.Failure(Error.Forbidden("Notification", "You can only mark your own notifications as read"));
        }

        notification.MarkAsRead(_clock);
        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
