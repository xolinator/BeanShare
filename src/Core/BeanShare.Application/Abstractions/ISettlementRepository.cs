using BeanShare.Domain.Aggregates.Settlement;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface ISettlementRepository
{
    Task<Settlement?> GetByIdAsync(SettlementId id, CancellationToken cancellationToken = default);
    Task<Settlement?> GetByBillingPeriodIdAsync(BillingPeriodId billingPeriodId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Settlement>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default);
    Task AddAsync(Settlement settlement, CancellationToken cancellationToken = default);
    Task UpdateAsync(Settlement settlement, CancellationToken cancellationToken = default);
}