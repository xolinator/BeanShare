using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;

namespace BeanShare.Application.Features.Consumption.Queries;

public sealed record GetRecentConsumptionsQuery(Guid SpaceId) : IQuery<Result<GetRecentConsumptionsResult>>;

public sealed record GetRecentConsumptionsResult(
    IEnumerable<ConsumptionEntryDto> Entries,
    int MyCupsToday,
    decimal MyTotalCost,
    int RemainingStockGrams
);
