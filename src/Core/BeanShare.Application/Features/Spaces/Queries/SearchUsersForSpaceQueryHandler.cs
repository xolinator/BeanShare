using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Queries;

public sealed class SearchUsersForSpaceQueryHandler
    : IRequestHandler<SearchUsersForSpaceQuery, Result<IReadOnlyList<UserSearchResultDto>>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserRepository _userRepository;

    public SearchUsersForSpaceQueryHandler(
        ISpaceRepository spaceRepository,
        IUserRepository userRepository)
    {
        _spaceRepository = spaceRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<IReadOnlyList<UserSearchResultDto>>> Handle(
        SearchUsersForSpaceQuery request,
        CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);

        if (space is null)
            return Result<IReadOnlyList<UserSearchResultDto>>.Failure(Error.SpaceNotFound(request.SpaceId));

        var existingMemberIds = space.Members.Select(m => m.UserId).ToHashSet();

        var (users, _) = await _userRepository.GetPagedAsync(
            page: 1,
            pageSize: request.MaxResults + existingMemberIds.Count,
            searchTerm: request.SearchTerm,
            roleFilter: null,
            activeFilter: true,
            ct: cancellationToken);

        var results = users
            .Where(u => !existingMemberIds.Contains(u.Id))
            .Take(request.MaxResults)
            .Select(u => new UserSearchResultDto(u.Id.Value, u.Name, u.Email))
            .ToList() as IReadOnlyList<UserSearchResultDto>;

        return Result<IReadOnlyList<UserSearchResultDto>>.Success(results);
    }
}
