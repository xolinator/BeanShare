using BeanShare.Application.Abstractions;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Analytics.Queries.GetUserStatistics;

public sealed class GetUserStatisticsQueryHandler : IRequestHandler<GetUserStatisticsQuery, UserStatisticsDto>
{
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly ICostCalculationService _costCalculationService;
    private readonly IClock _clock;

    public GetUserStatisticsQueryHandler(
        IConsumptionRepository consumptionRepository,
        ISpaceRepository spaceRepository,
        ICostCalculationService costCalculationService,
        IClock clock)
    {
        _consumptionRepository = consumptionRepository;
        _spaceRepository = spaceRepository;
        _costCalculationService = costCalculationService;
        _clock = clock;
    }

    public async Task<UserStatisticsDto> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var fromDate = request.FromDate ?? DateTime.MinValue;
        var toDate = request.ToDate ?? now;

        var spec = new ConsumptionsByUserSpecification(request.UserId, fromDate, toDate);
        var consumptions = await _consumptionRepository.GetBySpecAsync(spec, cancellationToken);

        if (!consumptions.Any())
        {
            return new UserStatisticsDto
            {
                TotalCups = 0,
                CupsThisMonth = 0,
                CupsThisWeek = 0,
                CupsToday = 0,
                AverageCupsPerDay = 0,
                TotalCost = null,
                ActiveSpacesCount = 0
            };
        }

        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek).ToUniversalTime();
        var startOfDay = now.Date.ToUniversalTime();

        var totalCups = consumptions.Count();
        var cupsThisMonth = consumptions.Count(c => c.ConsumedAt >= startOfMonth);
        var cupsThisWeek = consumptions.Count(c => c.ConsumedAt >= startOfWeek);
        var cupsToday = consumptions.Count(c => c.ConsumedAt >= startOfDay);

        var firstConsumption = consumptions.Min(c => c.ConsumedAt);
        var lastConsumption = consumptions.Max(c => c.ConsumedAt);
        var daysSinceFirst = (now - firstConsumption).Days + 1;
        var avgCupsPerDay = daysSinceFirst > 0 ? (double)totalCups / daysSinceFirst : 0;

        var coffeeTypeBreakdown = consumptions
            .GroupBy(c => c.Product.Name)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderByDescending(kvp => kvp.Value)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var mostConsumedType = coffeeTypeBreakdown.FirstOrDefault().Key;

        var last30Days = now.AddDays(-30).Date.ToUniversalTime();
        var dailyTrend = consumptions
            .Where(c => c.ConsumedAt >= last30Days)
            .GroupBy(c => c.ConsumedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count())
            .OrderBy(kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        Money? totalCost = null;
        var consumptionsBySpace = consumptions.GroupBy(c => c.SpaceId);

        foreach (var spaceGroup in consumptionsBySpace)
        {
            var spaceId = spaceGroup.Key;
            var totalGrams = spaceGroup.Sum(c => c.Quantity.Grams);

            var spaceCost = await _costCalculationService.CalculateTotalCostAsync(spaceId, totalGrams, cancellationToken);

            if (spaceCost != null)
            {
                if (totalCost == null)
                {
                    totalCost = spaceCost;
                }
                else
                {
                    if (totalCost.Currency.Code == spaceCost.Currency.Code)
                    {
                        totalCost = totalCost.Add(spaceCost);
                    }
                }
            }
        }

        var activeSpaces = consumptions.Select(c => c.SpaceId).Distinct().Count();

        return new UserStatisticsDto
        {
            TotalCups = totalCups,
            CupsThisMonth = cupsThisMonth,
            CupsThisWeek = cupsThisWeek,
            CupsToday = cupsToday,
            AverageCupsPerDay = Math.Round(avgCupsPerDay, 1),
            TotalCost = totalCost,
            MostConsumedCoffeeType = mostConsumedType,
            FirstConsumptionDate = firstConsumption,
            LastConsumptionDate = lastConsumption,
            CoffeeTypeBreakdown = coffeeTypeBreakdown,
            DailyConsumptionTrend = dailyTrend,
            ActiveSpacesCount = activeSpaces
        };
    }
}
