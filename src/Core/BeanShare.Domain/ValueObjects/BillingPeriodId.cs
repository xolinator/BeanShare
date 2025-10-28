namespace BeanShare.Domain.ValueObjects;

public readonly struct BillingPeriodId : IEquatable<BillingPeriodId>
{
    public Guid Value { get; }

    public BillingPeriodId(Guid value)
    {
        Value = value;
    }

    public static BillingPeriodId New() => new(Guid.NewGuid());

    public bool Equals(BillingPeriodId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is BillingPeriodId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(BillingPeriodId left, BillingPeriodId right) => left.Equals(right);
    public static bool operator !=(BillingPeriodId left, BillingPeriodId right) => !left.Equals(right);
}