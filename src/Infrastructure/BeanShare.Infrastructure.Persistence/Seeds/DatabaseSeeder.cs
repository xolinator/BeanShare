using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class DatabaseSeeder
{
    private readonly BeanShareDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IEnumerable<IDataSeeder> _seeders;

    public DatabaseSeeder(
        BeanShareDbContext context,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;

        _seeders = new List<IDataSeeder>
        {
            new UserSeeder(),
            new SpaceSeeder(),
            new CoffeeStockSeeder(),
            new ConsumptionSeeder()
        };
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            var orderedSeeders = _seeders.OrderBy(s => s.Order);

            foreach (var seeder in orderedSeeders)
            {
                var seederName = seeder.GetType().Name;
                _logger.LogInformation("Running seeder: {SeederName}", seederName);

                try
                {
                    await seeder.SeedAsync(_context, cancellationToken);
                    _logger.LogInformation("Successfully completed seeder: {SeederName}", seederName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running seeder: {SeederName}", seederName);
                    throw;
                }
            }

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database seeding failed");
            throw;
        }
    }
}

public static class DatabaseSeederExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BeanShareDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

        var seeder = new DatabaseSeeder(context, logger);
        await seeder.SeedAsync();
    }
}