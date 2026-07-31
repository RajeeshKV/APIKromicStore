using KromicStore.Domain.Orders.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for OrderItem value object.
/// </summary>
public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.OrderId)
            .IsRequired();

        builder.Property(oi => oi.ProductId)
            .IsRequired();

        builder.Property(oi => oi.ProductVariantId);

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(oi => oi.ProductSku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(oi => oi.VariantName)
            .HasMaxLength(256);

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        builder.Property(oi => oi.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(oi => oi.LineTotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(oi => oi.IsCancelled)
            .IsRequired();

        builder.Property(oi => oi.IsReturned)
            .IsRequired();

        builder.Property(oi => oi.ReturnedQuantity)
            .IsRequired();

        builder.Property(oi => oi.CancelledOnUtc);

        builder.Property(oi => oi.ReturnedOnUtc);

        // Indexes
        builder.HasIndex(oi => oi.OrderId)
            .HasDatabaseName("IX_OrderItem_OrderId");

        builder.HasIndex(oi => oi.ProductId)
            .HasDatabaseName("IX_OrderItem_ProductId");

        builder.ToTable("OrderItems");
    }
}
