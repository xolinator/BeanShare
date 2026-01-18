namespace BeanShare.Contracts.CoffeeStock;

public sealed record SplitRemainingStockRequest
{
    public string Reason { get; init; } = "Phantom stock adjustment";
}
