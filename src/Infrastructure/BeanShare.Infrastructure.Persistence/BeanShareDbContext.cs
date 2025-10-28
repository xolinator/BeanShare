using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Aggregates.BillingPeriod;
using BeanShare.Domain.Aggregates.Settlement;
using BeanShare.Domain.Entities;
using BeanShare.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence;

public sealed class BeanShareDbContext(DbContextOptions<BeanShareDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Space> Spaces => Set<Space>();
    public DbSet<CoffeeStock> CoffeeStocks => Set<CoffeeStock>();
    public DbSet<ConsumptionEntry> Consumptions => Set<ConsumptionEntry>();
    public DbSet<BillingPeriod> BillingPeriods => Set<BillingPeriod>();
    public DbSet<Settlement> Settlements => Set<Settlement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new SpaceConfiguration());
        modelBuilder.ApplyConfiguration(new CoffeeStockConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseConfiguration());
        modelBuilder.ApplyConfiguration(new StockLevelConfiguration());
        modelBuilder.ApplyConfiguration(new ConsumptionEntryConfiguration());
        modelBuilder.ApplyConfiguration(new BillingPeriodConfiguration());
        modelBuilder.ApplyConfiguration(new SettlementConfiguration());
    }
}