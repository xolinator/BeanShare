using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed record RemoveMemberCommand(Guid SpaceId, Guid UserId) : ICommand<Result<MemberActionDto>>;