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

        // Post-process to set the current user's actual role in each space
        var userId = _userContext.CurrentUserId;
        var spacesWithRoles = new List<SpaceSummaryDto>();
        for (int i = 0; i < spaceDtos.Count; i++)
        {
            var dto = spaceDtos[i];
            var space = userSpaces[i];
            var membership = space.Members.FirstOrDefault(m => m.UserId == userId);
            if (membership != null)
            {
                dto = dto with { Role = membership.Role.ToString() };
            }
            spacesWithRoles.Add(dto);
        }

        return Result<GetUserSpacesResult>.Success(new GetUserSpacesResult(spacesWithRoles));
    }
}