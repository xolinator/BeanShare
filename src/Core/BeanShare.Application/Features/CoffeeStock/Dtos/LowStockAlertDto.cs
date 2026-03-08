namespace BeanShare.Application.Features.CoffeeStock.Dtos;

public sealed class LowStockAlertDto
{
    public required Guid StockLevelId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required string ProductDisplayName { get; init; }
    public required decimal CurrentStockGrams { get; init; }
    public required decimal ThresholdGrams { get; init; }
    public required string AlertLevel { get; init; }
    public required DateTime LastUpdated { get; init; }
}