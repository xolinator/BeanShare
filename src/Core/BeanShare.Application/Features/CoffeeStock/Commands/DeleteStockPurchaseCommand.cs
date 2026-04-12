using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed record DeleteStockPurchaseCommand(
    Guid SpaceId,
    Guid PurchaseId
) : ICommand<Result>;
