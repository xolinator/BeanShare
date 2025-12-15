using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class SpaceGlobalPresetConfigConfiguration : IEntityTypeConfiguration<SpaceGlobalPresetConfig>
{
    public void Configure(EntityTypeBuilder<SpaceGlobalPresetConfig> builder)
    {
        builder.ToTable("SpaceGlobalPresetConfigs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SpaceGlobalPresetConfigId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.SpaceId)
            .HasConversion(
                id => id.Value,
                value => new SpaceId(value))
            .IsRequired();

        builder.Property(x => x.GlobalPresetId)
            .HasConversion(
                id => id.Value,
                value => new GlobalPresetId(value))
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamptz");

        builder.HasIndex(x => new { x.SpaceId, x.GlobalPresetId })
            .HasDatabaseName("IX_SpaceGlobalPresetConfigs_SpaceId_GlobalPresetId")
            .IsUnique();

        builder.HasIndex(x => x.SpaceId)
            .HasDatabaseName("IX_SpaceGlobalPresetConfigs_SpaceId");
    }
}
