using BeanShare.Domain.Common;

namespace BeanShare.Domain.Entities;

/// <summary>
/// Represents an exchange rate from a base currency (USD) to a target currency.
/// Used for converting amounts between different currencies in the dashboard.
/// </summary>
public sealed class ExchangeRate : Entity
{
    public ExchangeRateId Id { get; private set; }
    public string BaseCurrencyCode { get; private set; }
    public string TargetCurrencyCode { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime FetchedAt { get; private set; }

    private ExchangeRate()
    {
        Id = new ExchangeRateId(Guid.Empty);
        BaseCurrencyCode = string.Empty;
        TargetCurrencyCode = string.Empty;
    }

    private ExchangeRate(
        ExchangeRateId id,
        string baseCurrencyCode,
        string targetCurrencyCode,
        decimal rate,
        DateTime fetchedAt)
    {
        Id = id;
        BaseCurrencyCode = baseCurrencyCode.ToUpperInvariant();
        TargetCurrencyCode = targetCurrencyCode.ToUpperInvariant();
        Rate = rate;
        FetchedAt = fetchedAt;
    }

    public static ExchangeRate Create(
        string baseCurrencyCode,
        string targetCurrencyCode,
        decimal rate,
        DateTime fetchedAt)
    {
        if (string.IsNullOrWhiteSpace(baseCurrencyCode))
            throw new ArgumentException("Base currency code cannot be empty", nameof(baseCurrencyCode));

        if (string.IsNullOrWhiteSpace(targetCurrencyCode))
            throw new ArgumentException("Target currency code cannot be empty", nameof(targetCurrencyCode));

        if (rate <= 0)
            throw new ArgumentException("Exchange rate must be positive", nameof(rate));

        return new ExchangeRate(
            new ExchangeRateId(Guid.NewGuid()),
            baseCurrencyCode,
            targetCurrencyCode,
            rate,
            fetchedAt);
    }

    public void UpdateRate(decimal newRate, DateTime fetchedAt)
    {
        if (newRate <= 0)
            throw new ArgumentException("Exchange rate must be positive", nameof(newRate));

        Rate = newRate;
        FetchedAt = fetchedAt;
    }

    protected override object GetId() => Id;
}

public sealed record ExchangeRateId(Guid Value);
