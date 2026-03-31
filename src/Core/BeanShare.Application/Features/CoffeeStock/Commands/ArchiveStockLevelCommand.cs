using BeanShare.Application.Common;
using BeanShare.Application.Abstractions;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed record ArchiveStockLevelCommand(Guid SpaceId, Guid StockLevelId) : ICommand<Result<bool>>;
