namespace BeanShare.Contracts.Presets;

public sealed record DeletePresetResponse
{
    public required string Message { get; init; }
}
