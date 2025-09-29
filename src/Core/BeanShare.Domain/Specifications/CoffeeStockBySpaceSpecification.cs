using System.Linq.Expressions;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class CoffeeStockBySpaceSpecification : ISpec<CoffeeStock>
{
    private readonly SpaceId _spaceId;

    public CoffeeStockBySpaceSpecification(SpaceId spaceId)
    {
        if (spaceId.Value == Guid.Empty)
            throw new ArgumentException("SpaceId cannot be empty", nameof(spaceId));

        _spaceId = spaceId;
    }

    public Expression<Func<CoffeeStock, bool>> Criteria => cs => cs.SpaceId == _spaceId;

    public string? Reason => $"Coffee stock for space {_spaceId}";
}