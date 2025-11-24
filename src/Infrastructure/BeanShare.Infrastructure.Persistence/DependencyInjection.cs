using BeanShare.Application.Abstractions;
using BeanShare.Application.Services;
using BeanShare.Domain.Repositories;
using BeanShare.Infrastructure.Persistence.Repositories;
using BeanShare.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeanShare.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString, bool useInMemory = false)
    {
        if (useInMemory)
        {
            // Use in-memory database for testing
            services.AddDbContext<BeanShareDbContext>(options =>
                options.UseInMemoryDatabase("BeanShareInMemory")
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors()
                       .ConfigureWarnings(warnings =>
                           warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));
        }
        else
        {
            // Use PostgreSQL for production
            services.AddDbContext<BeanShareDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services.AddScoped<ISpaceRepository, SpaceRepository>();
        services.AddScoped<ICoffeeStockRepository, CoffeeStockRepository>();
        services.AddScoped<IConsumptionRepository, ConsumptionRepository>();
        services.AddScoped<IBillingPeriodRepository, BillingPeriodRepository>();
        services.AddScoped<ISettlementRepository, SettlementRepository>();
        services.AddScoped<IPresetRecipeRepository, PresetRecipeRepository>();
        services.AddScoped<IInviteCodeGenerator, InviteCodeGenerator>();
        services.AddScoped<ICostCalculationService, CostCalculationService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}