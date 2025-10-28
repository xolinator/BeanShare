using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;
using BeanShare.Application.Services;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Consumption.Queries.GetUserConsumptionHistory;

public sealed class GetUserConsumptionHistoryHandler : IRequestHandler<GetUserConsumptionHistoryQuery, Result<ConsumptionHistoryDto>>
{
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly ICostCalculationService _costCalculationService;
    private readonly IUserContext _userContext;

    public GetUserConsumptionHistoryHandler(
        IConsumptionRepository consumptionRepository,
        ISpaceRepository spaceRepository,
        ICostCalculationService costCalculationService,
        IUserContext userContext)
    {
        _consumptionRepository = consumptionRepository;
        _spaceRepository = spaceRepository;
        _costCalculationService = costCalculationService;
        _userContext = userContext;
    }

    public async Task<Result<ConsumptionHistoryDto>> Handle(GetUserConsumptionHistoryQuery query, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.CurrentUserId;

        var userSpacesSpec = new SpacesWithUserMembershipSpecification(currentUserId);
        var userSpaces = await _spaceRepository.GetBySpecAsync(userSpacesSpec, cancellationToken);

        if (!userSpaces.Any())
        {
            return Result<ConsumptionHistoryDto>.Success(new ConsumptionHistoryDto(
                0,
                query.PageNumber,
                query.PageSize,
                new List<ConsumptionHistoryItemDto>(),
                new ConsumptionHistorySummaryDto(0, 0, 0, 0, new Dictionary<string, int>(), new Dictionary<string, decimal>())
            ));
        }

        var allConsumptions = new List<Domain.Entities.ConsumptionEntry>();
        var spaces = new Dictionary<SpaceId, Domain.Aggregates.Space.Space>();

        foreach (var space in userSpaces)
        {
            if (query.SpaceId.HasValue && space.Id.Value != query.SpaceId.Value)
            {
                continue;
            }

            spaces[space.Id] = space;

            BillingPeriodId? billingPeriodId = query.BillingPeriodId.HasValue
                ? new BillingPeriodId(query.BillingPeriodId.Value)
                : (BillingPeriodId?)null;

            var consumptionSpec = new ConsumptionsByUserAndDateRangeSpecification(
                currentUserId,
                space.Id,
                query.StartDate,
                query.EndDate,
                billingPeriodId);

            var userConsumptions = await _consumptionRepository.GetBySpecAsync(consumptionSpec, cancellationToken);

            allConsumptions.AddRange(userConsumptions);
        }

        if (query.SpaceId.HasValue && !spaces.Any(s => s.Key.Value == query.SpaceId.Value))
        {
            var requestedSpaceId = new SpaceId(query.SpaceId.Value);
            var spaceByIdSpec = new SpaceByIdSpecification(requestedSpaceId);
            var requestedSpace = await _spaceRepository.GetSingleBySpecAsync(spaceByIdSpec, cancellationToken);

            if (requestedSpace == null)
            {
                return Result<ConsumptionHistoryDto>.Failure(Error.SpaceNotFound(query.SpaceId.Value));
            }

            return Result<ConsumptionHistoryDto>.Failure(Error.InsufficientSpacePrivileges("view consumption history"));
        }

        var totalCount = allConsumptions.Count;

        var skip = (query.PageNumber - 1) * query.PageSize;
        var paginatedConsumptions = allConsumptions
            .OrderByDescending(c => c.ConsumedAt)
            .Skip(skip)
            .Take(query.PageSize)
            .ToList();

        var items = new List<ConsumptionHistoryItemDto>();
        foreach (var c in paginatedConsumptions)
        {
            var spaceName = spaces.TryGetValue(c.SpaceId, out var space) ? space.Name : "Unknown Space";
            decimal? estimatedCost = null;
            string? currency = null;

            var cost = await _costCalculationService.CalculateConsumptionCostAsync(c.SpaceId, c.Quantity.Grams, cancellationToken);
            if (cost != null)
            {
                estimatedCost = cost.Amount;
                currency = cost.Currency.Code;
            }

            items.Add(new ConsumptionHistoryItemDto(
                c.Id.Value,
                c.SpaceId.Value,
                spaceName,
                c.Product.Name,
                c.Product.Brand,
                c.Product.Type.ToString(),
                c.Quantity.Grams,
                c.ConsumedAt,
                estimatedCost,
                currency,
                c.BillingPeriodId?.Value,
                null
            ));
        }

        var totalGrams = allConsumptions.Sum(c => c.Quantity.Grams);
        var totalEntries = allConsumptions.Count;
        var uniqueDays = allConsumptions.Select(c => c.ConsumedAt.Date).Distinct().Count();
        var averagePerDay = uniqueDays > 0 ? totalGrams / uniqueDays : 0;

        var consumptionByType = allConsumptions
            .GroupBy(c => c.Product.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var consumptionGramsBySpace = allConsumptions
            .GroupBy(c => c.SpaceId)
            .Select(g => new {
                SpaceName = spaces.TryGetValue(g.Key, out var s) ? s.Name : "Unknown",
                TotalGrams = g.Sum(c => c.Quantity.Grams)
            })
            .ToDictionary(x => x.SpaceName, x => x.TotalGrams);

        var summary = new ConsumptionHistorySummaryDto(
            totalGrams,
            totalEntries,
            uniqueDays,
            Math.Round(averagePerDay, 2),
            consumptionByType,
            consumptionGramsBySpace
        );

        var result = new ConsumptionHistoryDto(
            totalCount,
            query.PageNumber,
            query.PageSize,
            items,
            summary
        );

        return Result<ConsumptionHistoryDto>.Success(result);
    }
}