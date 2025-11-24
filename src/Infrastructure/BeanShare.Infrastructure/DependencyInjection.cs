using BeanShare.Application.Abstractions;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Services;
using BeanShare.Infrastructure.Persistence;
using BeanShare.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BeanShare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, bool useInMemoryDatabase = false)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ICostingPolicy, WeightedAverageCostingPolicy>();

        services.AddPersistence(connectionString, useInMemoryDatabase);

        services.AddScoped<IUserService, UserService>();
        services.AddMemoryCache();

        // services.AddIdentity();
        // services.AddCommunication();

        return services;
    }
}
