using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Services;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Commands.ClearNotifications;

public sealed class ClearNotificationsHandler : IRequestHandler<ClearNotificationsCommand, Result<int>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public ClearNotificationsHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(ClearNotificationsCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.CurrentUserId;
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);
        var count = notifications.Count;

        await _notificationRepository.DeleteAllByUserIdAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(count);
    }
}
