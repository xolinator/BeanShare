using System.Linq.Expressions;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class UnassignedConsumptionsInPeriodSpecification : ISpec<ConsumptionEntry>
{
    private readonly SpaceId _spaceId;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public UnassignedConsumptionsInPeriodSpecification(
        SpaceId spaceId,
        DateTime startDate,
        DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(spaceId);

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");

        _spaceId = spaceId;
        _startDate = startDate;
        _endDate = endDate;
    }

    public Expression<Func<ConsumptionEntry, bool>> Criteria => c =>
        c.SpaceId == _spaceId &&
        c.ConsumedAt >= _startDate &&
        c.ConsumedAt <= _endDate &&
        c.BillingPeriodId == null;

    public string? Reason => $"Unassigned consumptions in space {_spaceId.Value} between {_startDate:yyyy-MM-dd} and {_endDate:yyyy-MM-dd}";
}
