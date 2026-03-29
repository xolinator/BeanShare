using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class ActiveQrCodeConfiguration : IEntityTypeConfiguration<ActiveQrCode>
{
    public void Configure(EntityTypeBuilder<ActiveQrCode> builder)
    {
        builder.ToTable("ActiveQrCodes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ActiveQrCodeId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.SpaceId)
            .HasConversion(
                id => id.Value,
                value => new SpaceId(value))
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Product, productBuilder =>
        {
            productBuilder.WithOwner();

            productBuilder.Property(p => p.Name)
                .HasColumnName("ProductName")
                .HasMaxLength(200)
                .IsRequired();

            productBuilder.Property(p => p.Brand)
                .HasColumnName("ProductBrand")
                .HasMaxLength(200)
                .IsRequired();

            productBuilder.Property(p => p.Type)
                .HasColumnName("ProductType")
                .HasConversion<string>()
                .IsRequired();
        });

        builder.Property(x => x.RecipeName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.DefaultGrams)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.SpaceId)
            .HasDatabaseName("IX_ActiveQrCodes_SpaceId");

        builder.HasIndex(x => new { x.SpaceId, x.IsActive })
            .HasDatabaseName("IX_ActiveQrCodes_SpaceId_IsActive");
    }
}
