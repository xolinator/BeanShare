namespace BeanShare.Contracts.Presets;

public sealed record PresetDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required string Name { get; init; }
    public required string CoffeeType { get; init; }
    public required string Preparation { get; init; }
    public required decimal DefaultGrams { get; init; }
    public string? Notes { get; init; }
    public required bool IsShared { get; init; }
    public required bool IsOwner { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastUsedAt { get; init; }
    public required int UsageCount { get; init; }
}
