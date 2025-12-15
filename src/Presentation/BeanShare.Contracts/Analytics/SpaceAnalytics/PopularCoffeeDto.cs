namespace BeanShare.Contracts.Analytics.SpaceAnalytics;

public sealed record PopularCoffeeDto
{
    public required string CoffeeName { get; init; }
    public required int ConsumptionCount { get; init; }
    public required decimal TotalGrams { get; init; }
    public required double Percentage { get; init; }
}
