using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence;

public sealed class BeanShareDbContext : DbContext
{
    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<CoffeeStock> CoffeeStocks => Set<CoffeeStock>();

    public BeanShareDbContext(DbContextOptions<BeanShareDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new SpaceConfiguration());
        modelBuilder.ApplyConfiguration(new CoffeeStockConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseConfiguration());
        modelBuilder.ApplyConfiguration(new StockLevelConfiguration());
    }
}