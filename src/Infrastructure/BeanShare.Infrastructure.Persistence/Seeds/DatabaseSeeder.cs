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
            new GlobalPresetSeeder(),
            new UserSeeder(),
            new SpaceSeeder(),
            new CoffeeStockSeeder(),
            new ConsumptionSeeder(),
            new BillingPeriodSeeder()
        };
    }

    /// <summary>
    /// Seeds the database. In production mode (includeDemoData=false), only essential seeders
    /// run (e.g. global presets). In development (includeDemoData=true), all seeders run
    /// including demo users, spaces, consumption data, etc.
    /// </summary>
    public async Task SeedAsync(bool includeDemoData = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var mode = includeDemoData ? "development (all data)" : "production (essential only)";
            _logger.LogInformation("Starting database seeding in {Mode} mode...", mode);

            var seedersToRun = _seeders
                .Where(s => includeDemoData || s.IsEssential)
                .OrderBy(s => s.Order);

            foreach (var seeder in seedersToRun)
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