using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Queries.GetUnreadCount;

public sealed class GetUnreadCountHandler : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;

    public GetUnreadCountHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
    }

    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.CurrentUserId;
        var count = await _notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);
        return Result<int>.Success(count);
    }
}
