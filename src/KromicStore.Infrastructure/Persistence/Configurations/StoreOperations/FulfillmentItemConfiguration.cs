using KromicStore.Domain.StoreOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations.StoreOperations;

public sealed class FulfillmentItemConfiguration : IEntityTypeConfiguration<FulfillmentItem>
{
    public void Configure(EntityTypeBuilder<FulfillmentItem> builder)
    {
        builder.ToTable("fulfillment_items");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.FulfillmentId).HasColumnName("fulfillment_id").IsRequired();
        builder.Property(x => x.OrderLineItemId).HasColumnName("order_line_item_id").IsRequired();
        builder.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.SKU).HasColumnName("sku").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(x => x.PickedQuantity).HasColumnName("picked_quantity").IsRequired();
        builder.Property(x => x.PackedQuantity).HasColumnName("packed_quantity").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(10, 2).IsRequired();
        
        builder.Property(x => x.CreatedOnUtc).HasColumnName("created_on_utc").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnName("modified_on_utc").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        
        builder.HasIndex(x => new { x.TenantId, x.FulfillmentId }).HasDatabaseName("idx_fulfillment_items_fulfillment");
        builder.HasIndex(x => x.SKU).HasDatabaseName("idx_fulfillment_items_sku");
    }
}
