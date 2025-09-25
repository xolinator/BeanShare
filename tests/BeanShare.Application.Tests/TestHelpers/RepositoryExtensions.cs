using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Tests.TestHelpers;

public static class RepositoryExtensions
{
    public static async Task<Space?> GetByIdAsync(this ISpaceRepository repository, SpaceId id, CancellationToken cancellationToken = default)
    {
        var specification = new SpaceByIdSpecification(id);
        return await repository.GetSingleBySpecAsync(specification, cancellationToken);
    }

    public static async Task<Space?> GetByInviteCodeAsync(this ISpaceRepository repository, InviteCode code, CancellationToken cancellationToken = default)
    {
        var specification = new SpaceByInviteCodeSpecification(code);
        return await repository.GetSingleBySpecAsync(specification, cancellationToken);
    }

    public static async Task<IReadOnlyList<Space>> GetUserSpacesAsync(this ISpaceRepository repository, UserId userId, CancellationToken cancellationToken = default)
    {
        var specification = new SpacesWithUserMembershipSpecification(userId);
        return await repository.GetBySpecAsync(specification, cancellationToken);
    }
}