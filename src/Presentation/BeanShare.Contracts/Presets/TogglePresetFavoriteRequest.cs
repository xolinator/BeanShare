namespace BeanShare.Contracts.Presets;

public sealed record TogglePresetFavoriteRequest
{
    public Guid? GlobalPresetId { get; init; }
    public Guid? SpacePresetId { get; init; }
    public required bool IsFavorite { get; init; }
}
