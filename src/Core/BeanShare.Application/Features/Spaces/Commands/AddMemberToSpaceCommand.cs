using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;

namespace BeanShare.Application.Features.Spaces.Commands;

[RequireSpaceAdmin("SpaceId")]
public sealed record AddMemberToSpaceCommand(Guid SpaceId, Guid UserId) : ICommand<Result>, IAuthorize;
