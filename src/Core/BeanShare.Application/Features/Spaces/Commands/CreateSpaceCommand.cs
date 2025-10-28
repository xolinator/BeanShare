using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed record CreateSpaceCommand(string Name, string CurrencyCode) : ICommand<Result<CreateSpaceResult>>;

public sealed record CreateSpaceResult(Guid SpaceId, string InviteCode);