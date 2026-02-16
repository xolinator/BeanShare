using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Services;

public interface ICurrencyConversionService
{
    /// <summary>
    /// Converts a monetary amount to the target currency. Returns null if rates are unavailable.
    /// </summary>
    Task<Money?> ConvertAsync(Money amount, Currency targetCurrency, CancellationToken ct = default);

    /// <summary>
    /// Converts multiple amounts in different currencies to a single target currency with breakdown.
    /// </summary>
    Task<MultiCurrencyConversionResult> ConvertAllAsync(IEnumerable<Money> amounts, Currency targetCurrency, CancellationToken ct = default);

    Task<bool> AreRatesAvailableAsync(CancellationToken ct = default);
}

/// <param name="FullyConverted">Whether all amounts were successfully converted.</param>
/// <param name="ConvertedTotal">Total in target currency, or null if not fully converted.</param>
/// <param name="Breakdown">Per-currency conversion results.</param>
public sealed record MultiCurrencyConversionResult(
    bool FullyConverted,
    Money? ConvertedTotal,
    IReadOnlyList<CurrencyBreakdown> Breakdown);

/// <param name="Currency">The original currency.</param>
/// <param name="OriginalAmount">Amount before conversion.</param>
/// <param name="ConvertedAmount">Amount in target currency, or null if conversion failed.</param>
/// <param name="ConversionSuccessful">Whether this conversion succeeded.</param>
public sealed record CurrencyBreakdown(
    Currency Currency,
    Money OriginalAmount,
    Money? ConvertedAmount,
    bool ConversionSuccessful);
