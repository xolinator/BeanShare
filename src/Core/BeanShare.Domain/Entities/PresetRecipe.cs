using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

public sealed class PresetRecipe : Entity
{
    public PresetRecipeId Id { get; private set; }
    public UserId UserId { get; private set; }
    public SpaceId SpaceId { get; private set; }
    public string Name { get; private set; }
    public string CoffeeType { get; private set; }
    public string Brand { get; private set; }
    public string Preparation { get; private set; }
    public Weight DefaultGrams { get; private set; }
    public string? Notes { get; private set; }
    public bool IsShared { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public int UsageCount { get; private set; }

    private PresetRecipe()
    {
        Id = new PresetRecipeId(Guid.Empty);
        Name = string.Empty;
        CoffeeType = string.Empty;
        Brand = string.Empty;
        Preparation = string.Empty;
        DefaultGrams = Weight.Zero;
        UserId = default!;
        SpaceId = default!;
    }

    private PresetRecipe(
        PresetRecipeId id,
        UserId userId,
        SpaceId spaceId,
        string name,
        string coffeeType,
        string brand,
        string preparation,
        Weight defaultGrams,
        DateTime createdAt,
        string? notes = null,
        bool isShared = false)
    {
        Id = id;
        UserId = userId;
        SpaceId = spaceId;
        Name = name;
        CoffeeType = coffeeType;
        Brand = brand;
        Preparation = preparation;
        DefaultGrams = defaultGrams;
        Notes = notes;
        IsShared = isShared;
        CreatedAt = createdAt;
        LastUsedAt = null;
        UsageCount = 0;
    }

    public static PresetRecipe Create(
        UserId userId,
        SpaceId spaceId,
        string name,
        string coffeeType,
        string brand,
        string preparation,
        Weight defaultGrams,
        DateTime createdAt,
        string? notes = null,
        bool isShared = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(coffeeType))
            throw new ArgumentException("Coffee type is required", nameof(coffeeType));

        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand is required", nameof(brand));

        if (string.IsNullOrWhiteSpace(preparation))
            throw new ArgumentException("Preparation method is required", nameof(preparation));

        if (!defaultGrams.IsPositive)
            throw new ArgumentException("Default grams must be positive", nameof(defaultGrams));

        return new PresetRecipe(
            new PresetRecipeId(Guid.NewGuid()),
            userId,
            spaceId,
            name,
            coffeeType,
            brand,
            preparation,
            defaultGrams,
            createdAt,
            notes,
            isShared);
    }

    public void Update(
        string name,
        string coffeeType,
        string brand,
        string preparation,
        Weight defaultGrams,
        string? notes,
        bool isShared)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(coffeeType))
            throw new ArgumentException("Coffee type is required", nameof(coffeeType));

        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Brand is required", nameof(brand));

        if (string.IsNullOrWhiteSpace(preparation))
            throw new ArgumentException("Preparation method is required", nameof(preparation));

        if (!defaultGrams.IsPositive)
            throw new ArgumentException("Default grams must be positive", nameof(defaultGrams));

        Name = name;
        CoffeeType = coffeeType;
        Brand = brand;
        Preparation = preparation;
        DefaultGrams = defaultGrams;
        Notes = notes;
        IsShared = isShared;
    }

    public void RecordUsage(DateTime usedAt)
    {
        UsageCount++;
        LastUsedAt = usedAt;
    }

    public void UpdateSharing(bool isShared)
    {
        IsShared = isShared;
    }

    protected override object GetId() => Id;
}

public sealed record PresetRecipeId(Guid Value);