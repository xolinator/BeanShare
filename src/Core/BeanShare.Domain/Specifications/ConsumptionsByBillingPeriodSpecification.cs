using System.Linq.Expressions;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class ConsumptionsByBillingPeriodSpecification : ISpec<ConsumptionEntry>
{
    private readonly SpaceId _spaceId;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public ConsumptionsByBillingPeriodSpecification(SpaceId spaceId, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(spaceId);
        _spaceId = spaceId;
        _startDate = startDate;
        _endDate = endDate;
    }

    public Expression<Func<ConsumptionEntry, bool>> Criteria => c =>
        c.SpaceId == _spaceId &&
        c.ConsumedAt >= _startDate &&
        c.ConsumedAt <= _endDate;

    public string? Reason => $"Consumptions in space {_spaceId.Value} between {_startDate} and {_endDate}";
}
