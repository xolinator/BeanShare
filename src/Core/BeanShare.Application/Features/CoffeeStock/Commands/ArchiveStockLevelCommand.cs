using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed record ArchiveStockLevelCommand(Guid SpaceId, Guid StockLevelId) : IRequest<Result<bool>>;
