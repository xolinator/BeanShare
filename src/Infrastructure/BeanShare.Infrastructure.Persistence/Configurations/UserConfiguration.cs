using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.PictureUrl)
            .HasMaxLength(512)
            .IsRequired(false);

        builder.Property(u => u.Provider)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.ProviderUserId)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512)
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .IsRequired();

        builder.HasIndex(u => new { u.Provider, u.ProviderUserId })
            .IsUnique()
            .HasDatabaseName("IX_Users_Provider_ProviderUserId");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
    }
}
