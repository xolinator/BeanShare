using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed record DemoteMemberCommand(Guid SpaceId, Guid UserId) : ICommand<Result<MembershipDto>>;