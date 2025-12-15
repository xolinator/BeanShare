using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Services;

public interface ICurrencyConversionService
{
    Task<Money?> ConvertAsync(Money amount, Currency targetCurrency, CancellationToken ct = default);
    Task<MultiCurrencyConversionResult> ConvertAllAsync(IEnumerable<Money> amounts, Currency targetCurrency, CancellationToken ct = default);
    Task<bool> AreRatesAvailableAsync(CancellationToken ct = default);
}

public sealed record MultiCurrencyConversionResult(
    bool FullyConverted,
    Money? ConvertedTotal,
    IReadOnlyList<CurrencyBreakdown> Breakdown);

public sealed record CurrencyBreakdown(
    Currency Currency,
    Money OriginalAmount,
    Money? ConvertedAmount,
    bool ConversionSuccessful);
