namespace BeanShare.Infrastructure.Persistence.Seeds;

public interface IDataSeeder
{
    Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default);
    int Order { get; } // Determines the order of execution
}