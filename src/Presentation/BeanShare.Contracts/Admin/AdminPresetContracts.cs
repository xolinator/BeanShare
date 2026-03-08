namespace BeanShare.Contracts.Admin;

public sealed class GetAllGlobalPresetsResponse
{
    public required IReadOnlyList<GlobalPresetItem> Presets { get; set; }
}

public sealed class GlobalPresetItem
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string DefaultCoffeeType { get; set; }
    public required string DefaultPreparation { get; set; }
    public decimal DefaultGrams { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CreateGlobalPresetRequest
{
    public required string Name { get; set; }
    public required string DefaultCoffeeType { get; set; }
    public required string DefaultPreparation { get; set; }
    public decimal DefaultGrams { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class CreateGlobalPresetResponse
{
    public Guid Id { get; set; }
}

public sealed class UpdateGlobalPresetRequest
{
    public required string Name { get; set; }
    public required string DefaultCoffeeType { get; set; }
    public required string DefaultPreparation { get; set; }
    public decimal DefaultGrams { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class ToggleGlobalPresetActiveRequest
{
    public bool Activate { get; set; }
}
