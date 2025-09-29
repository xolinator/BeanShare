namespace BeanShare.Contracts.CoffeeStock;

public sealed record ConsumeStockRequest
{
    public required Guid SpaceId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required decimal QuantityGrams { get; init; }
}