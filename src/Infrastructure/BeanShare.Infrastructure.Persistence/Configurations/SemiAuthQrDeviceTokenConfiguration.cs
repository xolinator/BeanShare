using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class SemiAuthQrDeviceTokenConfiguration : IEntityTypeConfiguration<SemiAuthQrDeviceToken>
{
    public void Configure(EntityTypeBuilder<SemiAuthQrDeviceToken> builder)
    {
        builder.ToTable("SemiAuthQrDeviceTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        builder.Property(x => x.DeviceIdHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(x => x.RevocationReason)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.LastUsedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(x => x.LastUsedSpaceId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new SpaceId(value.Value))
            .IsRequired(false);

        builder.Property(x => x.LastUsedQrCodeId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new ActiveQrCodeId(value.Value))
            .IsRequired(false);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_SemiAuthQrDeviceTokens_TokenHash");

        builder.HasIndex(x => new { x.UserId, x.DeviceIdHash })
            .IsUnique()
            .HasDatabaseName("IX_SemiAuthQrDeviceTokens_User_Device");

        builder.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiresAt })
            .HasDatabaseName("IX_SemiAuthQrDeviceTokens_User_Revoked_ExpiresAt");
    }
}
