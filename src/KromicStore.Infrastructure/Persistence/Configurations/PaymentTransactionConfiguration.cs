using KromicStore.Domain.Orders.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PaymentTransaction value object.
/// </summary>
public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.PaymentId)
            .IsRequired();

        builder.Property(pt => pt.TransactionType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pt => pt.ProviderTransactionId)
            .HasMaxLength(256);

        builder.Property(pt => pt.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(pt => pt.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pt => pt.ResponseCode)
            .HasMaxLength(100);

        builder.Property(pt => pt.ResponseMessage)
            .HasMaxLength(500);

        builder.Property(pt => pt.RawResponse);
            // Column type is determined by database provider
            // - SQL Server: nvarchar(max)
            // - PostgreSQL: text

        builder.Property(pt => pt.CreatedOnUtc)
            .IsRequired();

        // Indexes
        builder.HasIndex(pt => pt.PaymentId)
            .HasDatabaseName("IX_PaymentTransaction_PaymentId");

        builder.HasIndex(pt => pt.ProviderTransactionId)
            .HasDatabaseName("IX_PaymentTransaction_ProviderTransactionId");

        builder.HasIndex(pt => pt.CreatedOnUtc)
            .HasDatabaseName("IX_PaymentTransaction_CreatedOnUtc");

        builder.ToTable("PaymentTransactions");
    }
}
