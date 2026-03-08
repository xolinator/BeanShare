using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MapsterMapper;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class DeactivateSpaceCommandHandler : IRequestHandler<DeactivateSpaceCommand, Result<SpaceDto>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;
    private readonly IClock _clock;

    public DeactivateSpaceCommandHandler(
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IMapper mapper,
        IClock clock)
    {
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _mapper = mapper;
        _clock = clock;
    }

    public async Task<Result<SpaceDto>> Handle(DeactivateSpaceCommand command, CancellationToken cancellationToken)
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
            return Result<SpaceDto>.Failure(Error.InsufficientSpacePrivileges("deactivate space"));
        }

        if (!space.IsActive)
        {
            return Result<SpaceDto>.Failure(Error.ValidationFailure(nameof(space.IsActive), "Space is already deactivated"));
        }

        space.Deactivate(_clock);
        await _spaceRepository.UpdateAsync(space, cancellationToken);

        var dto = _mapper.Map<SpaceDto>(space);
        return Result<SpaceDto>.Success(dto);
    }
}