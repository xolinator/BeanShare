using BeanShare.Domain.Aggregates.Space;
using BeanShare.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence;

public sealed class BeanShareDbContext : DbContext
{
    public DbSet<Space> Spaces => Set<Space>();

    public BeanShareDbContext(DbContextOptions<BeanShareDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new SpaceConfiguration());
    }
}