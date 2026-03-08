using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

/// <summary>
/// Represents a global preset that is seeded into the system and available to all spaces.
/// Space admins can choose to include/exclude these presets from their space.
/// </summary>
public sealed class GlobalPreset : Entity
{
    public GlobalPresetId Id { get; private set; }
    public string Name { get; private set; }
    public string DefaultCoffeeType { get; private set; }
    public string DefaultPreparation { get; private set; }
    public Weight DefaultGrams { get; private set; }
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private GlobalPreset()
    {
        Id = new GlobalPresetId(Guid.Empty);
        Name = string.Empty;
        DefaultCoffeeType = string.Empty;
        DefaultPreparation = string.Empty;
        DefaultGrams = Weight.Zero;
    }

    private GlobalPreset(
        GlobalPresetId id,
        string name,
        string defaultCoffeeType,
        string defaultPreparation,
        Weight defaultGrams,
        int displayOrder,
        DateTime createdAt,
        string? description = null)
    {
        Id = id;
        Name = name;
        DefaultCoffeeType = defaultCoffeeType;
        DefaultPreparation = defaultPreparation;
        DefaultGrams = defaultGrams;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public static GlobalPreset Create(
        string name,
        string defaultCoffeeType,
        string defaultPreparation,
        Weight defaultGrams,
        int displayOrder,
        DateTime createdAt,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(defaultCoffeeType))
            throw new ArgumentException("Coffee type is required", nameof(defaultCoffeeType));

        if (string.IsNullOrWhiteSpace(defaultPreparation))
            throw new ArgumentException("Preparation method is required", nameof(defaultPreparation));

        if (!defaultGrams.IsPositive)
            throw new ArgumentException("Default grams must be positive", nameof(defaultGrams));

        return new GlobalPreset(
            new GlobalPresetId(Guid.NewGuid()),
            name,
            defaultCoffeeType,
            defaultPreparation,
            defaultGrams,
            displayOrder,
            createdAt,
            description);
    }

    /// <summary>
    /// Creates a global preset with a specific ID (used for seeding with deterministic IDs).
    /// </summary>
    public static GlobalPreset CreateWithId(
        Guid id,
        string name,
        string defaultCoffeeType,
        string defaultPreparation,
        Weight defaultGrams,
        int displayOrder,
        DateTime createdAt,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(defaultCoffeeType))
            throw new ArgumentException("Coffee type is required", nameof(defaultCoffeeType));

        if (string.IsNullOrWhiteSpace(defaultPreparation))
            throw new ArgumentException("Preparation method is required", nameof(defaultPreparation));

        if (!defaultGrams.IsPositive)
            throw new ArgumentException("Default grams must be positive", nameof(defaultGrams));

        return new GlobalPreset(
            new GlobalPresetId(id),
            name,
            defaultCoffeeType,
            defaultPreparation,
            defaultGrams,
            displayOrder,
            createdAt,
            description);
    }

    public void Update(
        string name,
        string defaultCoffeeType,
        string defaultPreparation,
        Weight defaultGrams,
        string? description,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(defaultCoffeeType))
            throw new ArgumentException("Coffee type is required", nameof(defaultCoffeeType));

        if (string.IsNullOrWhiteSpace(defaultPreparation))
            throw new ArgumentException("Preparation method is required", nameof(defaultPreparation));

        if (!defaultGrams.IsPositive)
            throw new ArgumentException("Default grams must be positive", nameof(defaultGrams));

        Name = name;
        DefaultCoffeeType = defaultCoffeeType;
        DefaultPreparation = defaultPreparation;
        DefaultGrams = defaultGrams;
        Description = description;
        DisplayOrder = displayOrder;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    protected override object GetId() => Id;
}

public sealed record GlobalPresetId(Guid Value);
