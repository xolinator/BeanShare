namespace BeanShare.Domain.ValueObjects;

public sealed record ActiveQrCodeId
{
    public Guid Value { get; }

    public ActiveQrCodeId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ActiveQrCodeId cannot be empty", nameof(value));
        }

        Value = value;
    }

    public static ActiveQrCodeId New() => new(Guid.NewGuid());

    public static implicit operator Guid(ActiveQrCodeId id) => id.Value;
    public static explicit operator ActiveQrCodeId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
