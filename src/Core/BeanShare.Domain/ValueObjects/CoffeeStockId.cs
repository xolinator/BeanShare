namespace BeanShare.Domain.ValueObjects;

public sealed record CoffeeStockId(Guid Value)
{
    public static CoffeeStockId New() => new(Guid.NewGuid());
    public static CoffeeStockId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CoffeeStockId id) => id.Value;
    public static explicit operator CoffeeStockId(Guid value) => new(value);
}