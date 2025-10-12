using BeanShare.Application.Abstractions;
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
                       .EnableDetailedErrors());
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
        services.AddScoped<IInviteCodeGenerator, InviteCodeGenerator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}