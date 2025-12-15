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

    public async Task<ExchangeRatesResult> GetCurrentRatesAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AppId))
        {
            _logger.LogWarning("OpenExchangeRates AppId is not configured");
            return ExchangeRatesResult.Failure("AppId not configured");
        }

        try
        {
            var url = $"https://openexchangerates.org/api/latest.json?app_id={_options.AppId}";
            var response = await _httpClient.GetFromJsonAsync<OpenExchangeRatesResponse>(url, ct);

            if (response is null)
            {
                return ExchangeRatesResult.Failure("Empty response from API");
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
