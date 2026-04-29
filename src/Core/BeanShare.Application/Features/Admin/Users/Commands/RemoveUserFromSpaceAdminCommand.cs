using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Admin.Users.Commands;

public sealed record RemoveUserFromSpaceAdminCommand(Guid UserId, Guid SpaceId) : ICommand<Result>;
