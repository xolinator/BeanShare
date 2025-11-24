using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record SettlementLineAdded(
    SettlementId SettlementId,
    UserId UserId,
    decimal AmountDue,
    string Currency,
    DateTime AddedAt
) : IDomainEvent
{
    public DateTime OccurredOn => AddedAt;
}
