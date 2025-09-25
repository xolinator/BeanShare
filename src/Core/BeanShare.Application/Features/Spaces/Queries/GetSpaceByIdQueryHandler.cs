using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using MapsterMapper;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Queries;

public sealed class GetSpaceByIdQueryHandler : IRequestHandler<GetSpaceByIdQuery, Result<SpaceDto>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;

    public GetSpaceByIdQueryHandler(
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IMapper mapper)
    {
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _mapper = mapper;
    }

    public async Task<Result<SpaceDto>> Handle(GetSpaceByIdQuery request, CancellationToken cancellationToken)
    {
        var specification = new SpaceByIdSpecification(request.SpaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(specification, cancellationToken);

        if (space == null)
        {
            return Result<SpaceDto>.Failure(Error.SpaceNotFound(request.SpaceId.Value));
        }

        if (!space.HasMember(_userContext.CurrentUserId))
        {
            return Result<SpaceDto>.Failure(Error.InsufficientSpacePrivileges("view space details"));
        }

        var spaceDto = _mapper.Map<SpaceDto>(space);

        return Result<SpaceDto>.Success(spaceDto);
    }
}