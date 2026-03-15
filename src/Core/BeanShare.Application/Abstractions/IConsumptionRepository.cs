using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface IConsumptionRepository
{
    Task AddAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default);
    Task<ConsumptionEntry?> GetByIdAsync(ConsumptionEntryId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConsumptionEntry>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsumptionEntry>> GetBySpecAsync(ISpec<ConsumptionEntry> specification, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default);
    Task RemoveAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default);
}