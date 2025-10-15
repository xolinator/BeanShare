using System.Linq.Expressions;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class CoffeeStockByIdSpecification : ISpec<CoffeeStock>
{
    private readonly CoffeeStockId _id;

    public CoffeeStockByIdSpecification(CoffeeStockId id)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("CoffeeStockId cannot be empty", nameof(id));
        }

        _id = id;
    }

    public Expression<Func<CoffeeStock, bool>> Criteria => cs => cs.Id == _id;

    public string? Reason => $"Coffee stock with ID {_id.Value}";
}
