using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

/// <summary>
/// Domain event raised when a payment for a settlement line is confirmed.
/// </summary>
public sealed record PaymentConfirmed(
    SettlementId SettlementId,
    SpaceId SpaceId,
    UserId MemberUserId,
    UserId ConfirmedBy,
    DateTime ConfirmedAt
) : IDomainEvent
{
    public DateTime OccurredOn => ConfirmedAt;
}
