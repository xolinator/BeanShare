using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

[RequireSpaceAdmin("SpaceId")]
public sealed record SplitRemainingStockCommand(
    Guid SpaceId,
    Guid StockLevelId,
    string Reason = "Phantom stock adjustment"
) : IAuthorize, ICommand<Result<SplitRemainingStockResult>>;

public sealed record SplitRemainingStockResult(
    Guid SpaceId,
    Guid StockLevelId,
    string ProductName,
    string ProductBrand,
    string ProductType,
    decimal TotalDistributedGrams,
    IReadOnlyList<MemberAllocationResult> Allocations);

public sealed record MemberAllocationResult(
    Guid UserId,
    string UserName,
    decimal ConsumptionPercentage,
    decimal AllocatedGrams);
