using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Spaces.Commands;
public sealed record LeaveSpaceCommand(Guid SpaceId) : ICommand<Result<LeaveSpaceResult>>;

public sealed record LeaveSpaceResult(Guid SpaceId, string SpaceName);
