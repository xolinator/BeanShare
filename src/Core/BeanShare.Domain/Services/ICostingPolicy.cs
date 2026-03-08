using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Services;

/// <summary>
/// Strategy for calculating the cost of coffee consumption based on purchase history.
/// </summary>
public interface ICostingPolicy
{
    Money CalculateCost(
        IEnumerable<Purchase> purchases,
        Weight totalConsumed,
        string currency);

    string PolicyName { get; }
}
