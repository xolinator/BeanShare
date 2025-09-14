using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface ISpaceRepository
{
    Task<Space?> GetByIdAsync(SpaceId id, CancellationToken cancellationToken = default);
    Task<Space?> GetByInviteCodeAsync(InviteCode code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Space>> GetUserSpacesAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(Space space, CancellationToken cancellationToken = default);
    Task UpdateAsync(Space space, CancellationToken cancellationToken = default);
}