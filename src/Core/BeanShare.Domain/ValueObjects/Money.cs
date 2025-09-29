using System.Globalization;

namespace BeanShare.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; private init; }
    public string Currency { get; private init; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required", nameof(currency));
        }

        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency must be 3 characters", nameof(currency));
        }

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "USD") => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");
        }

        var result = Amount - other.Amount;
        if (result < 0)
        {
            throw new InvalidOperationException("Result cannot be negative");
        }

        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new ArgumentException("Factor cannot be negative", nameof(factor));
        }

        return new Money(Math.Round(Amount * factor, 2), Currency);
    }

    public Money Divide(decimal divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentException("Divisor must be positive", nameof(divisor));
        }

        return new Money(Math.Round(Amount / divisor, 2), Currency);
    }

    public override string ToString()
    {
        return $"{Amount:F2} {Currency}";
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");
        }

        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency} and {right.Currency}");
        }

        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right) => !(left < right);
    public static bool operator <=(Money left, Money right) => !(left > right);
}