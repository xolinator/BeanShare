using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;
using BeanShare.Application.Features.CoffeeStock.Dtos;

namespace BeanShare.Application.Features.CoffeeStock.Queries;
[RequireSpaceMember("SpaceId")]
public sealed record GetSpaceStockQuery(Guid SpaceId, int Page = 1, int PageSize = 10) : IAuthorize, IQuery<Result<CoffeeStockDto>>;