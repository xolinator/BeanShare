using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed record JoinSpaceCommand(string InviteCode) : ICommand<Result<JoinSpaceResult>>;

public sealed record JoinSpaceResult(Guid SpaceId, string SpaceName);