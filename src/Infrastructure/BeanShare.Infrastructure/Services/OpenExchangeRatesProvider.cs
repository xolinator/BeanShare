using System.Net.Http.Json;
using BeanShare.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BeanShare.Infrastructure.Services;

public sealed class OpenExchangeRatesProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenExchangeRatesOptions _options;
    private readonly ILogger<OpenExchangeRatesProvider> _logger;

    public OpenExchangeRatesProvider(
        HttpClient httpClient,
        IOptions<OpenExchangeRatesOptions> options,
        ILogger<OpenExchangeRatesProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Fallback rates used when no API key is configured. Based on approximate early-2026 values.
    /// </summary>
    private static readonly Dictionary<string, decimal> FallbackRates = new()
    {
        { "USD", 1.0m },
        { "EUR", 0.92m },
        { "GBP", 0.79m },
        { "CZK", 23.5m },
        { "PLN", 4.05m },
        { "CHF", 0.88m },
        { "JPY", 149.5m },
        { "CAD", 1.36m },
        { "AUD", 1.55m },
        { "SEK", 10.6m },
        { "NOK", 10.8m },
        { "DKK", 6.88m },
        { "HUF", 375.0m },
        { "RON", 4.58m },
        { "BGN", 1.80m },
        { "HRK", 6.93m },
        { "RUB", 92.0m },
        { "TRY", 30.5m },
        { "BRL", 5.0m },
        { "INR", 83.0m },
        { "CNY", 7.25m },
        { "KRW", 1320.0m },
    };

    public async Task<ExchangeRatesResult> GetCurrentRatesAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AppId))
        {
            _logger.LogInformation("OpenExchangeRates AppId not configured, using fallback constant rates");
            return new ExchangeRatesResult(true, "USD", FallbackRates, DateTime.UtcNow);
        }

        try
        {
            var url = $"https://openexchangerates.org/api/latest.json?app_id={_options.AppId}";
            var response = await _httpClient.GetFromJsonAsync<OpenExchangeRatesResponse>(url, ct);

            if (response is null)
            {
                return ExchangeRatesResult.Failure("Empty response from API");
            }

            if (response.Rates is null || response.Rates.Count == 0)
            {
                return ExchangeRatesResult.Failure("No exchange rates in API response");
            }

            var timestamp = DateTimeOffset.FromUnixTimeSeconds(response.Timestamp).UtcDateTime;

            _logger.LogInformation("Successfully fetched {Count} exchange rates", response.Rates.Count);

            return new ExchangeRatesResult(
                true,
                response.Base,
                response.Rates,
                timestamp);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching exchange rates");
            return ExchangeRatesResult.Failure($"HTTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rates");
            return ExchangeRatesResult.Failure(ex.Message);
        }
    }
}

public sealed class OpenExchangeRatesOptions
{
    public const string SectionName = "OpenExchangeRates";
    public string AppId { get; set; } = string.Empty;
}

internal sealed record OpenExchangeRatesResponse(
    long Timestamp,
    string Base,
    Dictionary<string, decimal> Rates);
