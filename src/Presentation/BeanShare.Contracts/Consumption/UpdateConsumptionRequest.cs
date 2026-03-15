namespace BeanShare.Contracts.Consumption;

public sealed record UpdateConsumptionRequest
{
    public Guid Id { get; init; }
    public required Guid SpaceId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required decimal QuantityGrams { get; init; }
    public required DateTime ConsumedAt { get; init; }
}
