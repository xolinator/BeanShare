using BeanShare.Application.Abstractions;
using BeanShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly BeanShareDbContext _context;

    public ExchangeRateRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRate?> GetRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default)
    {
        var baseCode = baseCurrency.ToUpperInvariant();
        var targetCode = targetCurrency.ToUpperInvariant();

        return await _context.ExchangeRates
            .FirstOrDefaultAsync(r =>
                r.BaseCurrencyCode == baseCode &&
                r.TargetCurrencyCode == targetCode, ct);
    }

    public async Task<IReadOnlyList<ExchangeRate>> GetAllRatesAsync(CancellationToken ct = default)
    {
        return await _context.ExchangeRates.ToListAsync(ct);
    }

    public async Task<DateTime?> GetLastFetchTimeAsync(CancellationToken ct = default)
    {
        return await _context.ExchangeRates
            .OrderByDescending(r => r.FetchedAt)
            .Select(r => (DateTime?)r.FetchedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task UpsertRateAsync(ExchangeRate rate, CancellationToken ct = default)
    {
        var existing = await GetRateAsync(rate.BaseCurrencyCode, rate.TargetCurrencyCode, ct);

        if (existing is not null)
        {
            existing.UpdateRate(rate.Rate, rate.FetchedAt);
        }
        else
        {
            await _context.ExchangeRates.AddAsync(rate, ct);
        }
    }

    public async Task UpsertRatesAsync(IEnumerable<ExchangeRate> rates, CancellationToken ct = default)
    {
        foreach (var rate in rates)
        {
            await UpsertRateAsync(rate, ct);
        }
    }
}
