using BeanShare.Application.Abstractions;
using BeanShare.Infrastructure.Communication.Configuration;
using BeanShare.Infrastructure.Communication.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeanShare.Infrastructure.Communication;

public static class DependencyInjection
{
    /// <summary>
    /// Adds communication services (email) to the service collection.
    /// </summary>
    public static IServiceCollection AddCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddSingleton<ISettlementEmailTemplateService, SettlementEmailTemplateService>();

        return services;
    }
}
