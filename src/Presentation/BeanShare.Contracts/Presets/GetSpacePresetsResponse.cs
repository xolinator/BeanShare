namespace BeanShare.Contracts.Presets;

public sealed record GetSpacePresetsResponse
{
    public required IReadOnlyCollection<PresetDto> Presets { get; init; }
}
