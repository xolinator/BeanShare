using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.CoffeeStock.Dtos;

namespace BeanShare.Application.Features.CoffeeStock.Queries;

public sealed record GetSpaceStockQuery(Guid SpaceId) : IQuery<Result<CoffeeStockDto>>;