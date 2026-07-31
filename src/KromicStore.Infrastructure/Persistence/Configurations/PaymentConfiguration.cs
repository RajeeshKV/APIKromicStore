using KromicStore.Domain.Orders.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Payment aggregate root.
/// </summary>
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.CustomerId)
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Provider)
            .HasMaxLength(50);

        builder.Property(p => p.ProviderTransactionId)
            .HasMaxLength(256);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.RefundedAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.AttemptCount)
            .IsRequired();

        builder.Property(p => p.MaxAttempts)
            .IsRequired();

        builder.Property(p => p.NextRetryAtUtc);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        builder.Property(p => p.FailureCode)
            .HasMaxLength(100);

        builder.Property(p => p.InitiatedOnUtc)
            .IsRequired();

        builder.Property(p => p.ProcessedOnUtc);

        builder.Property(p => p.RefundedOnUtc);

        // Auditing
        builder.Property(p => p.ModifiedAtUtc)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.ModifiedBy)
            .IsRequired()
            .HasMaxLength(256);

        // Soft delete
        builder.Property(p => p.IsDeleted)
            .IsRequired();

        builder.Property(p => p.DeletedOnUtc);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasMany<PaymentTransaction>()
            .WithOne()
            .HasForeignKey("PaymentId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.OrderId })
            .IsUnique()
            .HasDatabaseName("IX_Payment_TenantId_OrderId");

        builder.HasIndex(p => new { p.TenantId, p.CustomerId })
            .HasDatabaseName("IX_Payment_TenantId_CustomerId");

        builder.HasIndex(p => new { p.TenantId, p.Status })
            .HasDatabaseName("IX_Payment_TenantId_Status");

        builder.HasIndex(p => p.ProviderTransactionId)
            .HasDatabaseName("IX_Payment_ProviderTransactionId");

        builder.HasIndex(p => p.NextRetryAtUtc)
            .HasDatabaseName("IX_Payment_NextRetryAtUtc");

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Payment_IsDeleted");

        builder.ToTable("Payments");
    }
}
