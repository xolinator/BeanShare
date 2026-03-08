using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Consumption.Queries;
public sealed class GetRecentConsumptionsQueryHandler
    : IRequestHandler<GetRecentConsumptionsQuery, Result<GetRecentConsumptionsResult>>
{
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public GetRecentConsumptionsQueryHandler(
        IConsumptionRepository consumptionRepository,
        ICoffeeStockRepository coffeeStockRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _consumptionRepository = consumptionRepository;
        _coffeeStockRepository = coffeeStockRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<GetRecentConsumptionsResult>> Handle(
        GetRecentConsumptionsQuery request,
        CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var userId = _userContext.CurrentUserId;

        var spaceSpec = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);
        if (space == null)
        {
            return Result<GetRecentConsumptionsResult>.Failure(Error.SpaceNotFound(request.SpaceId));
        }

        if (!space.HasMember(userId))
        {
            return Result<GetRecentConsumptionsResult>.Failure(Error.InsufficientSpacePrivileges("view consumptions"));
        }

        var allEntries = await _consumptionRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        var recentEntries = allEntries
            .OrderByDescending(e => e.ConsumedAt)
            .Take(20)
            .Select(e => new ConsumptionEntryDto
            {
                Id = e.Id.Value,
                SpaceId = e.SpaceId.Value,
                UserId = e.UserId.Value,
                ProductName = e.Product?.Name ?? "Unknown",
                ProductBrand = e.Product?.Brand ?? "Unknown",
                ProductType = e.Product?.Type.ToString() ?? "Unknown",
                QuantityGrams = e.Quantity.Grams,
                RemainingGrams = 0,
                ConsumedAt = e.ConsumedAt,
                CreatedAt = e.CreatedAt,
                PresetId = e.PresetId?.Value,
                PresetName = e.PresetName
            })
            .ToList();

        var today = _clock.UtcNow.Date;
        var myEntriesToday = allEntries
            .Where(e => e.UserId == userId && e.ConsumedAt.Date == today)
            .ToList();

        var myCupsToday = myEntriesToday.Count;

        // Calculate actual total cost from user's consumption using weighted average cost per gram
        var myAllEntries = allEntries.Where(e => e.UserId == userId).ToList();
        var myTotalGrams = myAllEntries.Sum(e => e.Quantity.Grams);
        var myTotalCost = 0m;

        // Calculate remaining stock from actual stock data
        var remainingStock = 0;
        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        if (coffeeStock != null)
        {
            remainingStock = (int)coffeeStock.TotalCurrentStock.Grams;

            // Calculate cost per gram from stock purchases to estimate user's total cost
            var totalPurchaseCost = coffeeStock.Purchases.Sum(p => p.Cost.Amount);
            var totalPurchasedGrams = coffeeStock.Purchases.Sum(p => p.Quantity.Grams);
            if (totalPurchasedGrams > 0 && myTotalGrams > 0)
            {
                var costPerGram = totalPurchaseCost / totalPurchasedGrams;
                myTotalCost = costPerGram * myTotalGrams;
            }
        }

        var result = new GetRecentConsumptionsResult(
            recentEntries,
            myCupsToday,
            myTotalCost,
            remainingStock
        );

        return Result<GetRecentConsumptionsResult>.Success(result);
    }
}
