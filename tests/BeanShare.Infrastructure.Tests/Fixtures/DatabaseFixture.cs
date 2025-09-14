using BeanShare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace BeanShare.Infrastructure.Tests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("beanshare_test")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task<BeanShareDbContext> CreateDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<BeanShareDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var context = new BeanShareDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}