using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new NotificationId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasConversion(
                id => id.Value,
                value => (UserId)value)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.SpaceId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value.Value,
                value => value == null ? null : new SpaceId(value.Value))
            .IsRequired(false);

        builder.Property(x => x.IsRead)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.ReadAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        builder.Property(x => x.ActionUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.MetadataJson)
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Notifications_UserId");

        builder.HasIndex(x => new { x.UserId, x.IsRead })
            .HasDatabaseName("IX_Notifications_UserId_IsRead");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_Notifications_CreatedAt");
    }
}
