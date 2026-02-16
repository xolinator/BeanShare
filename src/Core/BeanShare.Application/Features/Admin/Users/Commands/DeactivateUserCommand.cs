using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Admin.Users.Commands;
public sealed record DeactivateUserCommand(Guid UserId) : ICommand<Result>;
