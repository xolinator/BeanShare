using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed record SetCurrentlyUsedStockLevelCommand(Guid SpaceId, Guid? StockLevelId) : ICommand<Result<bool>>;
