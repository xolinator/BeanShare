using System.Globalization;

namespace BeanShare.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; private init; }
    public Currency Currency { get; private init; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        if (currency == null)
        {
            throw new ArgumentException("Currency is required", nameof(currency));
        }

        var rounded = Math.Round(amount, currency.DecimalPlaces);
        return new Money(rounded, currency);
    }

    public static Money Zero(Currency currency) => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot add different currencies: {Currency.Code} and {other.Currency.Code}");
        }

        return Money.Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency.Code} and {other.Currency.Code}");
        }

        var result = Amount - other.Amount;
        if (result < 0)
        {
            throw new InvalidOperationException("Result cannot be negative");
        }

        return Money.Create(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new ArgumentException("Factor cannot be negative", nameof(factor));
        }

        return Money.Create(Amount * factor, Currency);
    }

    public Money Divide(decimal divisor)
    {
        if (divisor <= 0)
        {
            throw new ArgumentException("Divisor must be positive", nameof(divisor));
        }

        return Money.Create(Amount / divisor, Currency);
    }

    public override string ToString()
    {
        return Currency.FormatAmount(Amount);
    }

    public string Format()
    {
        return Currency.FormatAmount(Amount);
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency.Code} and {right.Currency.Code}");
        }

        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare different currencies: {left.Currency.Code} and {right.Currency.Code}");
        }

        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right) => !(left < right);
    public static bool operator <=(Money left, Money right) => !(left > right);
}