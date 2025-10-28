namespace BeanShare.Domain.ValueObjects;

public sealed record CoffeeProduct
{
    public string Name { get; init; }
    public string Brand { get; init; }
    public CoffeeType Type { get; init; }

    private CoffeeProduct()
    {
        Name = string.Empty;
        Brand = string.Empty;
        Type = CoffeeType.Espresso;
    }

    private CoffeeProduct(string name, string brand, CoffeeType type)
    {
        Name = name;
        Brand = brand;
        Type = type;
    }

    public static CoffeeProduct Create(string name, string brand, CoffeeType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Product name cannot exceed 100 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand is required", nameof(brand));

        if (brand.Length > 50)
            throw new ArgumentException("Brand cannot exceed 50 characters", nameof(brand));

        return new CoffeeProduct(name.Trim(), brand.Trim(), type);
    }

    public string DisplayName => $"{Brand} {Name}";

    public override string ToString() => $"{Brand} {Name} ({Type})";
}

public enum CoffeeType
{
    Espresso = 1,
    Filter = 2,
    Instant = 3,
    Decaf = 4,
    Specialty = 5
}