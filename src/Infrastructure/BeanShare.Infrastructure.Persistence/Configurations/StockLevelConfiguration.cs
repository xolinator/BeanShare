using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

public sealed class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("StockLevels");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id)
            .ValueGeneratedNever();

        builder.Property("CoffeeStockId")
            .HasConversion<Guid>()
            .IsRequired();

        builder.OwnsOne(sl => sl.Product, pb =>
        {
            pb.Property(cp => cp.Name)
                .HasColumnName("ProductName")
                .HasMaxLength(100)
                .IsRequired();

            pb.Property(cp => cp.Brand)
                .HasColumnName("ProductBrand")
                .HasMaxLength(50)
                .IsRequired();

            pb.Property(cp => cp.Type)
                .HasColumnName("ProductType")
                .HasConversion<string>()
                .IsRequired();
        });

        builder.OwnsOne(sl => sl.TotalPurchased, tpb =>
        {
            tpb.Property(w => w.Grams)
                .HasColumnName("TotalPurchasedGrams")
                .HasColumnType("decimal(10,1)")
                .IsRequired();
        });

        builder.OwnsOne(sl => sl.TotalConsumed, tcb =>
        {
            tcb.Property(w => w.Grams)
                .HasColumnName("TotalConsumedGrams")
                .HasColumnType("decimal(10,1)")
                .IsRequired();
        });

        builder.Property(sl => sl.IsArchived)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(sl => sl.UpdatedAt)
            .IsRequired();

        builder.HasIndex("CoffeeStockId");
        builder.HasIndex(nameof(StockLevel.UpdatedAt));
    }
}