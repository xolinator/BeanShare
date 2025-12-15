using BeanShare.Domain.Entities;

namespace BeanShare.Application.Abstractions;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default);
    Task<IReadOnlyList<ExchangeRate>> GetAllRatesAsync(CancellationToken ct = default);
    Task<DateTime?> GetLastFetchTimeAsync(CancellationToken ct = default);
    Task UpsertRateAsync(ExchangeRate rate, CancellationToken ct = default);
    Task UpsertRatesAsync(IEnumerable<ExchangeRate> rates, CancellationToken ct = default);
}
