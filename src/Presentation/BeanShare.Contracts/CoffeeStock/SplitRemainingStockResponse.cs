namespace BeanShare.Contracts.CoffeeStock;

public sealed record SplitRemainingStockResponse
{
    public required Guid SpaceId { get; init; }
    public required Guid StockLevelId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required decimal TotalDistributedGrams { get; init; }
    public required IReadOnlyList<MemberAllocationResponse> Allocations { get; init; }
    public required string Message { get; init; }
}

public sealed record MemberAllocationResponse
{
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required decimal ConsumptionPercentage { get; init; }
    public required decimal AllocatedGrams { get; init; }
}
