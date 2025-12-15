namespace BeanShare.Contracts.Presets;

public sealed record GetQuickPresetsResponse
{
    public required IReadOnlyCollection<QuickPresetDto> Presets { get; init; }
}
