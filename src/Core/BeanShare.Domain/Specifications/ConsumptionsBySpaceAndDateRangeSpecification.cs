using System.Linq.Expressions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class ConsumptionsBySpaceAndDateRangeSpecification : ISpec<ConsumptionEntry>
{
    private readonly SpaceId _spaceId;
    private readonly UserId? _userId;
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;
    private readonly BillingPeriodId? _billingPeriodId;

    public ConsumptionsBySpaceAndDateRangeSpecification(
        SpaceId spaceId,
        UserId? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        BillingPeriodId? billingPeriodId = null)
    {
        ArgumentNullException.ThrowIfNull(spaceId);

        _spaceId = spaceId;
        _userId = userId;
        _startDate = startDate;
        _endDate = endDate;
        _billingPeriodId = billingPeriodId;
    }

    public Expression<Func<ConsumptionEntry, bool>> Criteria => c =>
        c.SpaceId == _spaceId &&
        (_userId == null || c.UserId == _userId) &&
        (_startDate == null || c.ConsumedAt >= _startDate.Value) &&
        (_endDate == null || c.ConsumedAt <= _endDate.Value) &&
        (_billingPeriodId == null || c.BillingPeriodId == _billingPeriodId);

    public string? Reason => $"Consumptions in space {_spaceId.Value}" +
        (_userId != null ? $" for user {_userId.Value}" : " for all members") +
        (_startDate.HasValue ? $" from {_startDate.Value:yyyy-MM-dd}" : "") +
        (_endDate.HasValue ? $" to {_endDate.Value:yyyy-MM-dd}" : "") +
        (_billingPeriodId.HasValue ? $" in billing period {_billingPeriodId.Value.Value}" : "");
}
