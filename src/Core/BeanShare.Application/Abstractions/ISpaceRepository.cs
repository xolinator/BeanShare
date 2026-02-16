using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface ISpaceRepository
{
    Task<Space?> GetByIdAsync(SpaceId id, CancellationToken cancellationToken = default);
    Task<Space?> GetSingleBySpecAsync(ISpec<Space> specification, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Space>> GetBySpecAsync(ISpec<Space> specification, CancellationToken cancellationToken = default);
    Task AddAsync(Space space, CancellationToken cancellationToken = default);
    Task UpdateAsync(Space space, CancellationToken cancellationToken = default);
    Task<int> GetUserSpaceCountAsync(UserId userId, CancellationToken cancellationToken = default);
}