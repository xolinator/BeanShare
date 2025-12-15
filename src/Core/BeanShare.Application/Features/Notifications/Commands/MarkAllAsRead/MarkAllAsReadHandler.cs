using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed class MarkAllAsReadHandler : IRequestHandler<MarkAllAsReadCommand, Result<int>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllAsReadHandler(
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

    public async Task<Result<int>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.CurrentUserId;
        var unreadCount = await _notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);

        await _notificationRepository.MarkAllAsReadByUserIdAsync(userId, _clock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(unreadCount);
    }
}
