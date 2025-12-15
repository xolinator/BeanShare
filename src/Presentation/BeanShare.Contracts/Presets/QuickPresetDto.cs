namespace BeanShare.Contracts.Presets;

public sealed record QuickPresetDto
{
    public Guid? GlobalPresetId { get; init; }
    public Guid? SpacePresetId { get; init; }
    public required string Name { get; init; }
    public required string CoffeeType { get; init; }
    public required string Brand { get; init; }
    public required string Preparation { get; init; }
    public required decimal DefaultGrams { get; init; }
    public string? Description { get; init; }
    public required bool IsFavorite { get; init; }
    public required bool IsGlobal { get; init; }
    public required int DisplayOrder { get; init; }
}
