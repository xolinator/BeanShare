using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class PresetRecipeConfiguration : IEntityTypeConfiguration<PresetRecipe>
{
    public void Configure(EntityTypeBuilder<PresetRecipe> builder)
    {
        builder.ToTable("PresetRecipes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new PresetRecipeId(value))
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

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CoffeeType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Preparation)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DefaultGrams)
            .HasConversion(
                weight => weight.Grams,
                grams => Weight.FromGrams(grams))
            .HasColumnName("DefaultGrams")
            .HasColumnType("numeric(10,3)")
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.IsShared)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.LastUsedAt)
            .HasColumnType("timestamptz");

        builder.Property(x => x.UsageCount)
            .IsRequired();

        builder.HasIndex(x => x.SpaceId)
            .HasDatabaseName("IX_PresetRecipes_SpaceId");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_PresetRecipes_UserId");

        builder.HasIndex(x => new { x.SpaceId, x.UsageCount })
            .HasDatabaseName("IX_PresetRecipes_SpaceId_UsageCount")
            .IsDescending(false, true);
    }
}