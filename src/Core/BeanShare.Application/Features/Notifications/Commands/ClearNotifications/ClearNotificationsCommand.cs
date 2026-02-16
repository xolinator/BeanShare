using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Notifications.Commands.ClearNotifications;
public sealed record ClearNotificationsCommand() : ICommand<Result<int>>;
