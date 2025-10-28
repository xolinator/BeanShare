using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Services;

/// <summary>
/// Service for calculating costs of coffee consumption based on purchase prices
/// </summary>
public interface ICostCalculationService
{
    /// <summary>
    /// Calculates the cost of consumed coffee for a specific space
    /// </summary>
    /// <param name="spaceId">The space ID to calculate costs for</param>
    /// <param name="quantityGrams">The quantity consumed in grams</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Money representing the cost in the space's currency, or null if calculation not possible</returns>
    Task<Money?> CalculateConsumptionCostAsync(SpaceId spaceId, decimal quantityGrams, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the average cost per gram for a space based on all purchases
    /// </summary>
    /// <param name="spaceId">The space ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cost per gram as Money, or null if no purchases exist</returns>
    Task<Money?> GetAverageCostPerGramAsync(SpaceId spaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates total cost for multiple consumption entries in a space
    /// </summary>
    /// <param name="spaceId">The space ID</param>
    /// <param name="totalGrams">Total grams consumed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total cost as Money, or null if calculation not possible</returns>
    Task<Money?> CalculateTotalCostAsync(SpaceId spaceId, decimal totalGrams, CancellationToken cancellationToken = default);
}
