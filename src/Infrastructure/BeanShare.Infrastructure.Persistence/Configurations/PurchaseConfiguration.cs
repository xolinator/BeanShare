using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

public sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property("CoffeeStockId")
            .HasConversion<Guid>()
            .IsRequired();

        builder.OwnsOne(p => p.Product, pb =>
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

        builder.OwnsOne(p => p.Quantity, qb =>
        {
            qb.Property(w => w.Grams)
                .HasColumnName("QuantityGrams")
                .HasColumnType("decimal(10,1)")
                .IsRequired();
        });

        builder.OwnsOne(p => p.Cost, cb =>
        {
            cb.Property(m => m.Amount)
                .HasColumnName("CostAmount")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            cb.Property(m => m.Currency)
                .HasColumnName("CostCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(p => p.Vendor)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.PurchasedBy)
            .HasConversion(
                id => id.Value,
                value => (UserId)value)
            .IsRequired();

        builder.Property(p => p.PurchasedAt)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasIndex(p => p.PurchasedAt);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex("CoffeeStockId");
    }
}