using BeanShare.Domain.Entities;

namespace BeanShare.Application.Abstractions;

public interface IConsumptionRepository
{
    Task AddAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default);
}