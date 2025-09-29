namespace BeanShare.Contracts.CoffeeStock;

public sealed record GetSpaceStockRequest
{
    public required Guid SpaceId { get; init; }
}