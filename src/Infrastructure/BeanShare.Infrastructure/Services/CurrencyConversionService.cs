using BeanShare.Application.Abstractions;
using BeanShare.Application.Services;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BeanShare.Infrastructure.Services;

public sealed class CurrencyConversionService : ICurrencyConversionService
{
    private readonly IExchangeRateRepository _rateRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyConversionService> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MaxStaleness = TimeSpan.FromHours(24);
    private const string RatesCacheKey = "exchange_rates_all";

    public CurrencyConversionService(
        IExchangeRateRepository rateRepository,
        IMemoryCache cache,
        ILogger<CurrencyConversionService> logger)
    {
        _rateRepository = rateRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Money?> ConvertAsync(Money amount, Currency targetCurrency, CancellationToken ct = default)
    {
        if (amount.Currency.Code == targetCurrency.Code)
            return amount;

        var rates = await GetCachedRatesAsync(ct);
        if (rates.Count == 0)
        {
            _logger.LogWarning("No exchange rates available for conversion from {Source} to {Target}",
                amount.Currency.Code, targetCurrency.Code);
            return null;
        }

        var sourceToUsd = GetRateToUsd(amount.Currency.Code, rates);
        var usdToTarget = GetRateFromUsd(targetCurrency.Code, rates);

        if (sourceToUsd is null || usdToTarget is null)
        {
            _logger.LogWarning("Missing rate for conversion: {Source} -> USD ({SourceRate}) -> {Target} ({TargetRate})",
                amount.Currency.Code, sourceToUsd, targetCurrency.Code, usdToTarget);
            return null;
        }

        var usdAmount = amount.Amount / sourceToUsd.Value;
        var targetAmount = usdAmount * usdToTarget.Value;

        return Money.Create(targetAmount, targetCurrency);
    }

    public async Task<MultiCurrencyConversionResult> ConvertAllAsync(
        IEnumerable<Money> amounts,
        Currency targetCurrency,
        CancellationToken ct = default)
    {
        var breakdown = new List<CurrencyBreakdown>();
        var convertedAmounts = new List<Money>();
        var allConverted = true;

        foreach (var amount in amounts)
        {
            var converted = await ConvertAsync(amount, targetCurrency, ct);

            breakdown.Add(new CurrencyBreakdown(
                amount.Currency,
                amount,
                converted,
                converted is not null));

            if (converted is not null)
            {
                convertedAmounts.Add(converted);
            }
            else
            {
                allConverted = false;
            }
        }

        Money? total = null;
        if (convertedAmounts.Count > 0)
        {
            total = convertedAmounts.Aggregate((a, b) => a.Add(b));
        }

        return new MultiCurrencyConversionResult(allConverted, total, breakdown);
    }

    public async Task<bool> AreRatesAvailableAsync(CancellationToken ct = default)
    {
        var lastFetch = await _rateRepository.GetLastFetchTimeAsync(ct);
        if (lastFetch is null)
            return false;

        return DateTime.UtcNow - lastFetch.Value < MaxStaleness;
    }

    private async Task<IReadOnlyList<ExchangeRate>> GetCachedRatesAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue<IReadOnlyList<ExchangeRate>>(RatesCacheKey, out var cached) && cached is not null)
            return cached;

        var rates = await _rateRepository.GetAllRatesAsync(ct);
        _cache.Set(RatesCacheKey, rates, CacheDuration);
        return rates;
    }

    private static decimal? GetRateToUsd(string currencyCode, IReadOnlyList<ExchangeRate> rates)
    {
        if (currencyCode == "USD") return 1m;

        // Open Exchange Rates stores rates FROM USD TO other currencies
        // So to get the rate TO USD, we need to invert: 1 / (USD -> currency rate)
        var rate = rates.FirstOrDefault(r =>
            r.BaseCurrencyCode == "USD" && r.TargetCurrencyCode == currencyCode);

        return rate is not null ? 1m / rate.Rate : null;
    }

    private static decimal? GetRateFromUsd(string currencyCode, IReadOnlyList<ExchangeRate> rates)
    {
        if (currencyCode == "USD") return 1m;

        // Open Exchange Rates stores rates FROM USD TO other currencies directly
        return rates.FirstOrDefault(r =>
            r.BaseCurrencyCode == "USD" && r.TargetCurrencyCode == currencyCode)?.Rate;
    }
}
