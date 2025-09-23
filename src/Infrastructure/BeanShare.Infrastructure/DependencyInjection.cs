using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using BeanShare.Infrastructure.Persistence;
using BeanShare.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BeanShare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IClock, SystemClock>();

        services.AddPersistence(connectionString);

        // services.AddIdentity();
        // services.AddCommunication();

        return services;
    }
}
