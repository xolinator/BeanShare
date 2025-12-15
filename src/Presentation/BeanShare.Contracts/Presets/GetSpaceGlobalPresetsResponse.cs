namespace BeanShare.Contracts.Presets;

public sealed record GetSpaceGlobalPresetsResponse
{
    public required IReadOnlyCollection<SpaceGlobalPresetDto> Presets { get; init; }
}
