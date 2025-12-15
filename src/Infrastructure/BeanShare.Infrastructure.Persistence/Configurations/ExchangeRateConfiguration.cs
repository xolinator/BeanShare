using BeanShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeanShare.Infrastructure.Persistence.Configurations;

internal sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ExchangeRateId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.BaseCurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.TargetCurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Rate)
            .HasColumnType("decimal(18,10)")
            .IsRequired();

        builder.Property(x => x.FetchedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(x => new { x.BaseCurrencyCode, x.TargetCurrencyCode })
            .HasDatabaseName("IX_ExchangeRates_Base_Target")
            .IsUnique();
    }
}
