namespace BeanShare.Contracts.CoffeeStock;

public sealed record SpaceStockResponse
{
    public required Guid Id { get; init; }
    public required Guid SpaceId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required int PurchaseCount { get; init; }
    public required int ProductVarietyCount { get; init; }
    public required decimal TotalCurrentStockGrams { get; init; }
    public required decimal TotalInvestmentAmount { get; init; }
    public required string TotalInvestmentCurrency { get; init; }
    public required int TotalPurchaseCount { get; init; }
    public required IReadOnlyList<StockLevelResponse> StockLevels { get; init; }
    public required IReadOnlyList<StockPurchaseResponse> RecentPurchases { get; init; }
}