namespace BeanShare.Contracts.Consumption;

public sealed record RecordConsumptionFromPresetRequest
{
    public required Guid PresetId { get; init; }
    public decimal? CustomQuantityGrams { get; init; }
    public DateTime? ConsumedAt { get; init; }
}