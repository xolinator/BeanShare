using System.Linq.Expressions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class ConsumptionsByUserAndDateRangeSpecification : ISpec<ConsumptionEntry>
{
    private readonly UserId _userId;
    private readonly SpaceId _spaceId;
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;
    private readonly BillingPeriodId? _billingPeriodId;

    public ConsumptionsByUserAndDateRangeSpecification(
        UserId userId,
        SpaceId spaceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        BillingPeriodId? billingPeriodId = null)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(spaceId);

        _userId = userId;
        _spaceId = spaceId;
        _startDate = startDate;
        _endDate = endDate;
        _billingPeriodId = billingPeriodId;
    }

    public Expression<Func<ConsumptionEntry, bool>> Criteria => c =>
        c.UserId == _userId &&
        c.SpaceId == _spaceId &&
        (_startDate == null || c.ConsumedAt >= _startDate.Value) &&
        (_endDate == null || c.ConsumedAt <= _endDate.Value) &&
        (_billingPeriodId == null || c.BillingPeriodId == _billingPeriodId);

    public string? Reason => $"Consumptions for user {_userId.Value} in space {_spaceId.Value}" +
        (_startDate.HasValue ? $" from {_startDate.Value:yyyy-MM-dd}" : "") +
        (_endDate.HasValue ? $" to {_endDate.Value:yyyy-MM-dd}" : "") +
        (_billingPeriodId.HasValue ? $" in billing period {_billingPeriodId.Value.Value}" : "");
}
