namespace BeanShare.Contracts.Presets;

public sealed record UpdatePresetRequest
{
    public required string Name { get; init; }
    public required string CoffeeType { get; init; }
    public required string Preparation { get; init; }
    public required decimal DefaultGrams { get; init; }
    public string? Notes { get; init; }
    public bool IsShared { get; init; } = false;
}

public sealed record UpdatePresetResponse
{
    public required string Message { get; init; }
}