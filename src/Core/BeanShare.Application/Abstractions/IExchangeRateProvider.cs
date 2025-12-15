namespace BeanShare.Application.Abstractions;

public interface IExchangeRateProvider
{
    Task<ExchangeRatesResult> GetCurrentRatesAsync(CancellationToken ct = default);
}

public sealed record ExchangeRatesResult(
    bool Success,
    string BaseCurrency,
    IReadOnlyDictionary<string, decimal> Rates,
    DateTime Timestamp,
    string? ErrorMessage = null)
{
    public static ExchangeRatesResult Failure(string errorMessage) =>
        new(false, "USD", new Dictionary<string, decimal>(), DateTime.UtcNow, errorMessage);
}
