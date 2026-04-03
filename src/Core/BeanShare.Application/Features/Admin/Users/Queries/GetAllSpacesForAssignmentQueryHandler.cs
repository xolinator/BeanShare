using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Specifications;
using MediatR;

namespace BeanShare.Application.Features.Admin.Users.Queries;
public sealed class GetAllSpacesForAssignmentQueryHandler
    : IRequestHandler<GetAllSpacesForAssignmentQuery, Result<IReadOnlyList<SpaceOptionDto>>>
{
    private readonly ISpaceRepository _spaceRepository;

    public GetAllSpacesForAssignmentQueryHandler(ISpaceRepository spaceRepository)
    {
        _spaceRepository = spaceRepository;
    }

    public async Task<Result<IReadOnlyList<SpaceOptionDto>>> Handle(
        GetAllSpacesForAssignmentQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new ActiveSpacesSpecification();
        var spaces = await _spaceRepository.GetBySpecAsync(specification, cancellationToken);

        var options = spaces
            .OrderBy(s => s.Name)
            .Select(s => new SpaceOptionDto(s.Id.Value, s.Name))
            .ToList() as IReadOnlyList<SpaceOptionDto>;

        return Result<IReadOnlyList<SpaceOptionDto>>.Success(options);
    }
}