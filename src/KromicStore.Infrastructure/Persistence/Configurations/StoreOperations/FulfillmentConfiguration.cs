using KromicStore.Domain.StoreOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations.StoreOperations;

public sealed class FulfillmentConfiguration : IEntityTypeConfiguration<Fulfillment>
{
    public void Configure(EntityTypeBuilder<Fulfillment> builder)
    {
        builder.ToTable("fulfillments");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired();
        
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.ProcessedAtUtc).HasColumnName("processed_at_utc");
        builder.Property(x => x.PackedAtUtc).HasColumnName("packed_at_utc");
        builder.Property(x => x.ShippedAtUtc).HasColumnName("shipped_at_utc");
        builder.Property(x => x.DeliveredAtUtc).HasColumnName("delivered_at_utc");
        builder.Property(x => x.CancelledAtUtc).HasColumnName("cancelled_at_utc");
        
        builder.Property(x => x.TrackingNumber).HasColumnName("tracking_number").HasMaxLength(100);
        builder.Property(x => x.CarrierCode).HasColumnName("carrier_code").HasMaxLength(20);
        builder.Property(x => x.ShippingAddress).HasColumnName("shipping_address").HasMaxLength(300).IsRequired();
        builder.Property(x => x.ShippingCost).HasColumnName("shipping_cost").HasPrecision(10, 2).IsRequired();
        
        builder.Property(x => x.ProcessingNotes).HasColumnName("processing_notes").HasMaxLength(500);
        builder.Property(x => x.PackingNotes).HasColumnName("packing_notes").HasMaxLength(500);
        builder.Property(x => x.ShippingNotes).HasColumnName("shipping_notes").HasMaxLength(500);
        
        builder.Property(x => x.CreatedOnUtc).HasColumnName("created_on_utc").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnName("modified_on_utc").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(x => x.DeletedOnUtc).HasColumnName("deleted_on_utc");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(256);
        
        builder.HasMany<FulfillmentItem>()
            .WithOne()
            .HasForeignKey(x => x.FulfillmentId);
        
        builder.HasIndex(x => new { x.TenantId, x.OrderId }).IsUnique().HasDatabaseName("idx_fulfillments_order");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_fulfillments_status");
        builder.HasIndex(x => x.CreatedAtUtc).HasDatabaseName("idx_fulfillments_created");
    }
}
