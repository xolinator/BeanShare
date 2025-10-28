namespace BeanShare.Domain.ValueObjects;

public readonly struct SettlementId : IEquatable<SettlementId>
{
    public Guid Value { get; }

    public SettlementId(Guid value)
    {
        Value = value;
    }

    public static SettlementId New() => new(Guid.NewGuid());

    public bool Equals(SettlementId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is SettlementId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(SettlementId left, SettlementId right) => left.Equals(right);
    public static bool operator !=(SettlementId left, SettlementId right) => !left.Equals(right);
}