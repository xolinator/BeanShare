namespace BeanShare.Contracts.CoffeeStock;

public sealed record StockLevelResponse
{
    public required Guid Id { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required string ProductDisplayName { get; init; }
    public required decimal TotalPurchasedGrams { get; init; }
    public required decimal TotalConsumedGrams { get; init; }
    public required decimal CurrentStockGrams { get; init; }
    public required decimal ConsumptionPercentage { get; init; }
    public required bool IsArchived { get; init; }
    public required DateTime UpdatedAt { get; init; }
}