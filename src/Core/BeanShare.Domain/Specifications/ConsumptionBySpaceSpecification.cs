using System.Linq.Expressions;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class ConsumptionBySpaceSpecification : ISpec<ConsumptionEntry>
{
    private readonly SpaceId _spaceId;

    public ConsumptionBySpaceSpecification(SpaceId spaceId)
    {
        if (spaceId.Value == Guid.Empty)
        {
            throw new ArgumentException("SpaceId cannot be empty", nameof(spaceId));
        }

        _spaceId = spaceId;
    }

    public Expression<Func<ConsumptionEntry, bool>> Criteria => c => c.SpaceId == _spaceId;

    public string? Reason => $"Consumption entries for space {_spaceId.Value}";
}
