using KromicStore.Domain.Orders.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Order aggregate root.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId)
            .IsRequired();

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.BillingAddressId)
            .IsRequired();

        builder.Property(o => o.ShippingAddressId)
            .IsRequired();

        builder.Property(o => o.ShippingMethod)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.PaymentMethod)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(o => o.SubTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.DiscountAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.TaxAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.GrandTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.CouponCode)
            .HasMaxLength(50);

        builder.Property(o => o.Notes)
            .HasMaxLength(500);

        builder.Property(o => o.CreatedOnUtc)
            .IsRequired();

        builder.Property(o => o.ShippedOnUtc);

        builder.Property(o => o.DeliveredOnUtc);

        builder.Property(o => o.CancelledOnUtc);

        builder.Property(o => o.PaymentId);

        // Auditing
        builder.Property(o => o.ModifiedOnUtc)
            .HasColumnName("ModifiedAtUtc")
            .IsRequired();

        builder.Property(o => o.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.ModifiedBy)
            .IsRequired()
            .HasMaxLength(256);

        // Soft delete
        builder.Property(o => o.IsDeleted)
            .IsRequired();

        builder.Property(o => o.DeletedOnUtc);

        builder.Property(o => o.DeletedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasMany<OrderItem>()
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<OrderTimeline>()
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<OrderNote>()
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber })
            .IsUnique()
            .HasDatabaseName("IX_Order_TenantId_OrderNumber");

        builder.HasIndex(o => new { o.TenantId, o.CustomerId })
            .HasDatabaseName("IX_Order_TenantId_CustomerId");

        builder.HasIndex(o => new { o.TenantId, o.Status })
            .HasDatabaseName("IX_Order_TenantId_Status");

        builder.HasIndex(o => o.CreatedOnUtc)
            .HasDatabaseName("IX_Order_CreatedOnUtc");

        builder.HasIndex(o => o.IsDeleted)
            .HasDatabaseName("IX_Order_IsDeleted");

        builder.ToTable("Orders");
    }
}
