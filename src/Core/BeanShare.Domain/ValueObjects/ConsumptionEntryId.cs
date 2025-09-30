namespace BeanShare.Domain.ValueObjects;

public sealed record ConsumptionEntryId
{
    public Guid Value { get; }

    public ConsumptionEntryId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ConsumptionEntryId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static ConsumptionEntryId New() => new(Guid.NewGuid());

    public static implicit operator Guid(ConsumptionEntryId id) => id.Value;
    public static explicit operator ConsumptionEntryId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}