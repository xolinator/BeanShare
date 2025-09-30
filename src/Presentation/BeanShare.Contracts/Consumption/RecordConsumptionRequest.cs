namespace BeanShare.Contracts.Consumption;

public sealed record RecordConsumptionRequest
{
    public required Guid SpaceId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required decimal QuantityGrams { get; init; }
    public DateTime? ConsumedAt { get; init; }
}