using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllAsReadCommand() : ICommand<Result<int>>;
