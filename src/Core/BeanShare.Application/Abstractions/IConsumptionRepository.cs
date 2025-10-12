using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface IConsumptionRepository
{
    Task AddAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConsumptionEntry>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default);
}