namespace BeanShare.Contracts.CoffeeStock;

public sealed record GetSpaceStockRequest
{
    public required Guid SpaceId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}