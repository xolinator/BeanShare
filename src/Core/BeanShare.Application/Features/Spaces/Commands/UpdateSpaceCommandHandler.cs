using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MapsterMapper;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Commands;
public sealed class UpdateSpaceCommandHandler : IRequestHandler<UpdateSpaceCommand, Result<SpaceDto>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;

    public UpdateSpaceCommandHandler(
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IMapper mapper)
    {
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _mapper = mapper;
    }

    public async Task<Result<SpaceDto>> Handle(UpdateSpaceCommand command, CancellationToken cancellationToken)
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
            return Result<SpaceDto>.Failure(Error.InsufficientSpacePrivileges("update space"));
        }

        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Length > 100)
        {
            return Result<SpaceDto>.Failure(Error.InvalidSpaceName(command.Name));
        }

        try
        {
            space.UpdateName(command.Name.Trim());
            await _spaceRepository.UpdateAsync(space, cancellationToken);

            var dto = _mapper.Map<SpaceDto>(space);
            return Result<SpaceDto>.Success(dto);
        }
        catch (Exception)
        {
            return Result<SpaceDto>.Failure(Error.SystemFailure("update space"));
        }
    }
}