namespace BeanShare.Contracts.CoffeeStock;

public sealed record GetLowStockAlertsRequest
{
    public required Guid SpaceId { get; init; }
    public decimal ThresholdGrams { get; init; } = 100;
}