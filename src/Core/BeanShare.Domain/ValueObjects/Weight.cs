namespace BeanShare.Domain.ValueObjects;

public sealed record Weight
{
    public decimal Grams { get; private init; }

    private Weight(decimal grams)
    {
        Grams = grams;
    }

    public static Weight FromGrams(decimal grams)
    {
        if (grams < 0)
        {
            throw new ArgumentException("Weight cannot be negative", nameof(grams));
        }

        return new Weight(Math.Round(grams, 1));
    }

    public static Weight Zero => new(0);

    public Weight Add(Weight other)
    {
        return FromGrams(Grams + other.Grams);
    }

    public Weight Subtract(Weight other)
    {
        var result = Grams - other.Grams;
        if (result < 0)
        {
            throw new InvalidOperationException("Result cannot be negative");
        }

        return FromGrams(result);
    }

    public Weight Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new ArgumentException("Factor cannot be negative", nameof(factor));
        }

        return new Weight(Math.Round(Grams * factor, 1));
    }

    public bool IsPositive => Grams > 0;
    public bool IsZero => Grams == 0;

    public override string ToString()
    {
        return $"{Grams:F1}g";
    }

    public static bool operator >(Weight left, Weight right) => left.Grams > right.Grams;
    public static bool operator <(Weight left, Weight right) => left.Grams < right.Grams;
    public static bool operator >=(Weight left, Weight right) => left.Grams >= right.Grams;
    public static bool operator <=(Weight left, Weight right) => left.Grams <= right.Grams;
}