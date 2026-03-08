namespace BeanShare.Application.Features.Analytics.Queries.GetSpaceAnalytics;
public sealed record PopularCoffeeDto
{
    public string CoffeeName { get; init; } = string.Empty;
    public int ConsumptionCount { get; init; }
    public decimal TotalGrams { get; init; }
    public double Percentage { get; init; }
}
