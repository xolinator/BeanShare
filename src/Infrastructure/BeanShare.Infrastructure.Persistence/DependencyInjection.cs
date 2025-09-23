using BeanShare.Application.Abstractions;
using BeanShare.Infrastructure.Persistence.Repositories;
using BeanShare.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BeanShare.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BeanShareDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISpaceRepository, SpaceRepository>();
        services.AddScoped<IInviteCodeGenerator, InviteCodeGenerator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}