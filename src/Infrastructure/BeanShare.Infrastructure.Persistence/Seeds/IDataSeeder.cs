namespace BeanShare.Infrastructure.Persistence.Seeds;

public interface IDataSeeder
{
    Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default);
    int Order { get; } // Determines the order of execution

    /// <summary>
    /// Essential seeders run in all environments (including production).
    /// Non-essential seeders only run in development to provide demo data.
    /// </summary>
    bool IsEssential => false;
}