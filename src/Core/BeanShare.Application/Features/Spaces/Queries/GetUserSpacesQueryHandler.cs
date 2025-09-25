using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using MapsterMapper;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Queries;

public sealed class GetUserSpacesQueryHandler : IRequestHandler<GetUserSpacesQuery, Result<GetUserSpacesResult>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;

    public GetUserSpacesQueryHandler(
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IMapper mapper)
    {
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _mapper = mapper;
    }

    public async Task<Result<GetUserSpacesResult>> Handle(GetUserSpacesQuery request, CancellationToken cancellationToken)
    {
        var specification = new SpacesWithUserMembershipSpecification(_userContext.CurrentUserId);
        var userSpaces = await _spaceRepository.GetBySpecAsync(specification, cancellationToken);

        var spaceDtos = _mapper.Map<IReadOnlyList<SpaceSummaryDto>>(userSpaces);

        return Result<GetUserSpacesResult>.Success(new GetUserSpacesResult(spaceDtos));
    }
}