using BeanShare.Application.Abstractions;
using BeanShare.Application.Constants;
using BeanShare.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BeanShare.Infrastructure.Services;

public sealed class ExchangeRateUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExchangeRateUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(CacheSettings.ExchangeRateUpdateIntervalHours);

    public ExchangeRateUpdateService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExchangeRateUpdateService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Exchange rate update service starting");

        await UpdateRatesAsync(stoppingToken);

        using var timer = new PeriodicTimer(_updateInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await UpdateRatesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Exchange rate update service stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates, will retry in {Interval}", _updateInterval);
            }
        }
    }

    private async Task UpdateRatesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IExchangeRateProvider>();
        var repository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        _logger.LogInformation("Fetching exchange rates from provider...");

        var result = await provider.GetCurrentRatesAsync(ct);
        // Console.WriteLine($"DEBUG: Got {result.Rates?.Count ?? 0} rates from provider, success={result.Success}"); // keeping for debugging API issues

        if (!result.Success)
        {
            _logger.LogWarning("Failed to fetch exchange rates: {Error}", result.ErrorMessage);
            return;
        }

        var rates = result.Rates
            .Select(kvp => ExchangeRate.Create(
                result.BaseCurrency,
                kvp.Key,
                kvp.Value,
                result.Timestamp))
            .ToList();

        await repository.UpsertRatesAsync(rates, ct);
        await unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated {Count} exchange rates at {Time}", rates.Count, result.Timestamp);
    }
}
