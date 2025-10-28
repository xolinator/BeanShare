using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Services;

public interface ICostingPolicy
{
    Money CalculateCost(
        IEnumerable<Purchase> purchases,
        Weight totalConsumed,
        string currency);

    string PolicyName { get; }
}