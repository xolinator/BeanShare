namespace BeanShare.Contracts.CoffeeStock;

public sealed record ConsumeStockResponse
{
    public required Guid SpaceId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required decimal ConsumedGrams { get; init; }
    public required decimal RemainingGrams { get; init; }
    public required string Message { get; init; }
}