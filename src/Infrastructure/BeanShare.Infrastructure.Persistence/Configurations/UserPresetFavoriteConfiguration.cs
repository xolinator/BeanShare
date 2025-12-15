using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class UserPresetFavoriteConfiguration : IEntityTypeConfiguration<UserPresetFavorite>
{
    public void Configure(EntityTypeBuilder<UserPresetFavorite> builder)
    {
        builder.ToTable("UserPresetFavorites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new UserPresetFavoriteId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        builder.Property(x => x.SpaceId)
            .HasConversion(
                id => id.Value,
                value => new SpaceId(value))
            .IsRequired();

        builder.Property(x => x.GlobalPresetId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new GlobalPresetId(value.Value));

        builder.Property(x => x.PresetRecipeId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new PresetRecipeId(value.Value));

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.SpaceId })
            .HasDatabaseName("IX_UserPresetFavorites_UserId_SpaceId");

        builder.HasIndex(x => new { x.UserId, x.SpaceId, x.GlobalPresetId })
            .HasDatabaseName("IX_UserPresetFavorites_UserId_SpaceId_GlobalPresetId")
            .IsUnique()
            .HasFilter("\"GlobalPresetId\" IS NOT NULL");

        builder.HasIndex(x => new { x.UserId, x.SpaceId, x.PresetRecipeId })
            .HasDatabaseName("IX_UserPresetFavorites_UserId_SpaceId_PresetRecipeId")
            .IsUnique()
            .HasFilter("\"PresetRecipeId\" IS NOT NULL");
    }
}
