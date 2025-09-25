using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MapsterMapper;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class RegenerateInviteCodeCommandHandler : IRequestHandler<RegenerateInviteCodeCommand, Result<SpaceDto>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IInviteCodeGenerator _inviteCodeGenerator;
    private readonly IMapper _mapper;

    public RegenerateInviteCodeCommandHandler(
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IInviteCodeGenerator inviteCodeGenerator,
        IMapper mapper)
    {
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _inviteCodeGenerator = inviteCodeGenerator;
        _mapper = mapper;
    }

    public async Task<Result<SpaceDto>> Handle(RegenerateInviteCodeCommand command, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(command.SpaceId);
        var specification = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(specification, cancellationToken);

        if (space == null)
        {
            return Result<SpaceDto>.Failure(Error.SpaceNotFound(command.SpaceId));
        }

        var currentUserId = _userContext.CurrentUserId;
        if (!space.IsAdmin(currentUserId))
        {
            return Result<SpaceDto>.Failure(Error.InsufficientSpacePrivileges("regenerate invite code"));
        }

        if (!space.IsActive)
        {
            return Result<SpaceDto>.Failure(Error.ValidationFailure("IsActive", "Cannot regenerate invite code for deactivated space"));
        }

        try
        {
            var newInviteCode = await _inviteCodeGenerator.GenerateAsync(cancellationToken);
            space.RegenerateInviteCode(newInviteCode);

            await _spaceRepository.UpdateAsync(space, cancellationToken);

            var dto = _mapper.Map<SpaceDto>(space);
            return Result<SpaceDto>.Success(dto);
        }
        catch (Exception)
        {
            return Result<SpaceDto>.Failure(Error.SystemFailure("regenerate invite code"));
        }
    }
}