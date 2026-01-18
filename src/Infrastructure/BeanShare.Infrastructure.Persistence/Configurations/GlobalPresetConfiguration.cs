using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class GlobalPresetConfiguration : IEntityTypeConfiguration<GlobalPreset>
{
    public void Configure(EntityTypeBuilder<GlobalPreset> builder)
    {
        builder.ToTable("GlobalPresets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new GlobalPresetId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DefaultCoffeeType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DefaultPreparation)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DefaultGrams)
            .HasConversion(
                weight => weight.Grams,
                grams => Weight.FromGrams(grams))
            .HasColumnName("DefaultGrams")
            .HasColumnType("numeric(10,3)")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => x.DisplayOrder)
            .HasDatabaseName("IX_GlobalPresets_DisplayOrder");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_GlobalPresets_IsActive");
    }
}
