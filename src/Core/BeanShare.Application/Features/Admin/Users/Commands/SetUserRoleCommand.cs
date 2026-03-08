using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Enums;

namespace BeanShare.Application.Features.Admin.Users.Commands;
public sealed record SetUserRoleCommand(Guid UserId, SystemRole Role) : ICommand<Result>;
