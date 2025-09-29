namespace BeanShare.Contracts.CoffeeStock;

public sealed record LowStockAlertsResponse
{
    public required Guid SpaceId { get; init; }
    public required int AlertCount { get; init; }
    public required IReadOnlyList<LowStockAlertResponse> Alerts { get; init; }
}