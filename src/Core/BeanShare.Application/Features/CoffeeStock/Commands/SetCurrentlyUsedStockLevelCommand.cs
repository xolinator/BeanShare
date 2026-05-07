using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

/// <summary>
/// Sets or clears the "currently used" flag for a stock level within a space.
/// Pass null for <paramref name="StockLevelId"/> to clear any existing selection.
/// Only space admins can invoke this command.
/// </summary>
public sealed record SetCurrentlyUsedStockLevelCommand(Guid SpaceId, Guid? StockLevelId) : ICommand<Result<bool>>;
