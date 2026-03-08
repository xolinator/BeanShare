using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;
using BeanShare.Application.Features.CoffeeStock.Dtos;

namespace BeanShare.Application.Features.CoffeeStock.Queries;

[RequireSpaceMember("SpaceId")]
public sealed record GetSpaceStockQuery(Guid SpaceId) : IAuthorize, IQuery<Result<CoffeeStockDto>>;