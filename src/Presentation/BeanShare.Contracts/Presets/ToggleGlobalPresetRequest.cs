namespace BeanShare.Contracts.Presets;

public sealed record ToggleGlobalPresetRequest
{
    public required bool IsEnabled { get; init; }
}
