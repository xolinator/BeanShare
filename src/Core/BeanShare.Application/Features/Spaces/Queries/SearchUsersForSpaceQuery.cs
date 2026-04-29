using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;

namespace BeanShare.Application.Features.Spaces.Queries;

public sealed record UserSearchResultDto(Guid Id, string Name, string Email);

[RequireSpaceAdmin("SpaceId")]
public sealed record SearchUsersForSpaceQuery(Guid SpaceId, string SearchTerm, int MaxResults = 10)
    : IQuery<Result<IReadOnlyList<UserSearchResultDto>>>, IAuthorize;
