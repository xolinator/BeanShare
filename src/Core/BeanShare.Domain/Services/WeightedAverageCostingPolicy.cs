using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Services;

public sealed class WeightedAverageCostingPolicy : ICostingPolicy
{
    public string PolicyName => "Weighted Average";

    public Money CalculateCost(
        IEnumerable<Purchase> purchases,
        Weight totalConsumed,
        string currency)
    {
        ArgumentNullException.ThrowIfNull(purchases);
        ArgumentNullException.ThrowIfNull(totalConsumed);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var purchasesList = purchases.ToList();

        if (!purchasesList.Any())
        {
            return Money.Create(0, currency);
        }

        var mismatchedCurrency = purchasesList.FirstOrDefault(p => p.Cost.Currency.Code != currency);
        if (mismatchedCurrency != null)
        {
            throw new InvalidOperationException(
                $"Purchase currency mismatch: expected '{currency}' but found '{mismatchedCurrency.Cost.Currency.Code}'");
        }

        if (totalConsumed.IsZero)
        {
            return Money.Create(0, currency);
        }

        var totalCost = purchasesList.Sum(p => p.Cost.Amount);
        var totalWeight = purchasesList.Sum(p => p.Quantity.Grams);

        if (totalWeight == 0)
        {
            return Money.Create(0, currency);
        }

        var costPerGram = totalCost / totalWeight;
        var totalCostForConsumed = costPerGram * totalConsumed.Grams;

        return Money.Create(Math.Round(totalCostForConsumed, 2), currency);
    }
}