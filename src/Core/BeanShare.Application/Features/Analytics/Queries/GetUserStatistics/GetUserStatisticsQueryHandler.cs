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
    private readonly ICurrencyConversionService _currencyConversionService;
    private readonly IUserService _userService;
    private readonly IClock _clock;

    public GetUserStatisticsQueryHandler(
        IConsumptionRepository consumptionRepository,
        ISpaceRepository spaceRepository,
        ICostCalculationService costCalculationService,
        ICurrencyConversionService currencyConversionService,
        IUserService userService,
        IClock clock)
    {
        _consumptionRepository = consumptionRepository;
        _spaceRepository = spaceRepository;
        _costCalculationService = costCalculationService;
        _currencyConversionService = currencyConversionService;
        _userService = userService;
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
        var daysToMonday = now.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)now.DayOfWeek - 1;
        var startOfWeek = now.Date.AddDays(-daysToMonday).ToUniversalTime();
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

        var user = await _userService.GetByIdAsync(request.UserId, cancellationToken);
        string? preferredCurrencyCode = user?.PreferredCurrencyCode;

        var spaceCosts = new List<Money>();
        var consumptionsBySpace = consumptions.GroupBy(c => c.SpaceId);

        foreach (var spaceGroup in consumptionsBySpace)
        {
            var spaceId = spaceGroup.Key;
            var totalGrams = spaceGroup.Sum(c => c.Quantity.Grams);

            var spaceCost = await _costCalculationService.CalculateTotalCostAsync(spaceId, totalGrams, cancellationToken);

            if (spaceCost != null)
            {
                spaceCosts.Add(spaceCost);
                preferredCurrencyCode ??= spaceCost.Currency.Code;
            }
        }

        // Count actual space memberships, not just spaces with consumption
        var spacesSpec = new SpacesWithUserMembershipSpecification(request.UserId);
        var memberSpaces = await _spaceRepository.GetBySpecAsync(spacesSpec, cancellationToken);
        var activeSpaces = memberSpaces.Count;

        Money? totalCost = null;
        bool isCostFullyConverted = false;
        var costBreakdown = new List<CurrencyBreakdownDto>();

        if (spaceCosts.Count > 0 && preferredCurrencyCode != null)
        {
            var targetCurrency = Currency.Create(preferredCurrencyCode);
            var conversionResult = await _currencyConversionService.ConvertAllAsync(spaceCosts, targetCurrency, cancellationToken);

            isCostFullyConverted = conversionResult.FullyConverted;
            totalCost = conversionResult.ConvertedTotal;

  
            costBreakdown = conversionResult.Breakdown
                .Select(b => new CurrencyBreakdownDto
                {
                    CurrencyCode = b.Currency.Code,
                    OriginalAmount = b.OriginalAmount.Amount,
                    ConvertedAmount = b.ConvertedAmount,
                    WasConverted = b.ConversionSuccessful
                })
                .ToList();
        }

        return new UserStatisticsDto
        {
            TotalCups = totalCups,
            CupsThisMonth = cupsThisMonth,
            CupsThisWeek = cupsThisWeek,
            CupsToday = cupsToday,
            AverageCupsPerDay = Math.Round(avgCupsPerDay, 1),
            TotalCost = totalCost,
            IsCostFullyConverted = isCostFullyConverted,
            CostBreakdown = costBreakdown,
            PreferredCurrencyCode = preferredCurrencyCode,
            MostConsumedCoffeeType = mostConsumedType,
            FirstConsumptionDate = firstConsumption,
            LastConsumptionDate = lastConsumption,
            CoffeeTypeBreakdown = coffeeTypeBreakdown,
            DailyConsumptionTrend = dailyTrend,
            ActiveSpacesCount = activeSpaces
        };
    }
}
