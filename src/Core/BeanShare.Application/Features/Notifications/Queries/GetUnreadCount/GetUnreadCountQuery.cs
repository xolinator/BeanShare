using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Notifications.Queries.GetUnreadCount;
public sealed record GetUnreadCountQuery() : IRequest<Result<int>>;
