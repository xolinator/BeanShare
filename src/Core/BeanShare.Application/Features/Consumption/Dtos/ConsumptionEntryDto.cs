namespace BeanShare.Application.Features.Consumption.Dtos;

public sealed class ConsumptionEntryDto
{
    public required Guid Id { get; init; }
    public required Guid SpaceId { get; init; }
    public required Guid UserId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required decimal QuantityGrams { get; init; }
    public required decimal RemainingGrams { get; init; }
    public required DateTime ConsumedAt { get; init; }
    public required DateTime CreatedAt { get; init; }

    public string? PresetName { get; init; }
    public int CoffeeGrams => (int)QuantityGrams;
}