using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BeanShare.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BeanShareDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=beanshare_dev;Username=beanshare;Password=beanshare123";

    public BeanShareDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BeanShareDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("BEANSHARE_CONNECTION_STRING")
            ?? DefaultConnectionString;

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly(typeof(BeanShareDbContext).Assembly.FullName);
        });

        return new BeanShareDbContext(optionsBuilder.Options);
    }
}