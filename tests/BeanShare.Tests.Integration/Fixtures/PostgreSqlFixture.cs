using BeanShare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace BeanShare.Tests.Integration.Fixtures;

public sealed class PostgreSqlFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("beanshare_test")
        .WithUsername("beanshare_test")
        .WithPassword("beanshare_test123")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => _postgresContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<BeanShareDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test database context
            services.AddDbContext<BeanShareDbContext>(options =>
            {
                options.UseNpgsql(ConnectionString);
            });

            // Force real services (not mocks) for integration tests
            builder.UseSetting("UseMockServices", "false");
        });

        builder.UseEnvironment("Testing");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // Run migrations on the test database
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeanShareDbContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}