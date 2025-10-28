using BeanShare.Application.Abstractions;
using BeanShare.Application.Services;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Infrastructure.Persistence.Services;

/// <summary>
/// Service for calculating costs of coffee consumption based on purchase prices
/// </summary>
public sealed class CostCalculationService : ICostCalculationService
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly ICoffeeStockRepository _coffeeStockRepository;

    public CostCalculationService(
        ISpaceRepository spaceRepository,
        ICoffeeStockRepository coffeeStockRepository)
    {
        _spaceRepository = spaceRepository;
        _coffeeStockRepository = coffeeStockRepository;
    }

    public async Task<Money?> CalculateConsumptionCostAsync(
        SpaceId spaceId,
        decimal quantityGrams,
        CancellationToken cancellationToken = default)
    {
        var costPerGram = await GetAverageCostPerGramAsync(spaceId, cancellationToken);

        if (costPerGram == null)
            return null;

        return costPerGram.Multiply(quantityGrams);
    }

    public async Task<Money?> GetAverageCostPerGramAsync(
        SpaceId spaceId,
        CancellationToken cancellationToken = default)
    {
        // Get the space to determine its currency
        var spaceSpec = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);

        if (space == null)
            return null;

        // Get the stock for this space
        var stock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);

        if (stock == null || !stock.Purchases.Any())
            return null;

        // Calculate weighted average cost per gram across all purchases
        var totalCostAmount = stock.Purchases.Sum(p => p.Cost.Amount);
        var totalGrams = stock.Purchases.Sum(p => p.Quantity.Grams);

        if (totalGrams <= 0)
            return null;

        var averageCostPerGram = totalCostAmount / totalGrams;

        return Money.Create(averageCostPerGram, space.Currency);
    }

    public async Task<Money?> CalculateTotalCostAsync(
        SpaceId spaceId,
        decimal totalGrams,
        CancellationToken cancellationToken = default)
    {
        return await CalculateConsumptionCostAsync(spaceId, totalGrams, cancellationToken);
    }
}
