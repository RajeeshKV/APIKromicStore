using KromicStore.Domain.StoreOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations.StoreOperations;

public sealed class InventoryAdjustmentConfiguration : IEntityTypeConfiguration<InventoryAdjustment>
{
    public void Configure(EntityTypeBuilder<InventoryAdjustment> builder)
    {
        builder.ToTable("inventory_adjustments");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(x => x.AdjustmentQuantity).HasColumnName("adjustment_quantity").IsRequired();
        builder.Property(x => x.Reason).HasColumnName("reason").IsRequired();
        builder.Property(x => x.ReasonNotes).HasColumnName("reason_notes").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired();
        
        builder.Property(x => x.RequestedOnUtc).HasColumnName("requested_on_utc").IsRequired();
        builder.Property(x => x.RequestedBy).HasColumnName("requested_by").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ApprovedOnUtc).HasColumnName("approved_on_utc");
        builder.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(256);
        builder.Property(x => x.RejectedOnUtc).HasColumnName("rejected_on_utc");
        builder.Property(x => x.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
        builder.Property(x => x.AppliedOnUtc).HasColumnName("applied_on_utc");
        
        builder.Property(x => x.CreatedOnUtc).HasColumnName("created_on_utc").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnName("modified_on_utc").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(x => x.DeletedOnUtc).HasColumnName("deleted_on_utc");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(256);
        
        builder.HasIndex(x => new { x.TenantId, x.ProductId }).HasDatabaseName("idx_inventory_adjustments_product");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_inventory_adjustments_status");
        builder.HasIndex(x => x.RequestedOnUtc).HasDatabaseName("idx_inventory_adjustments_requested");
    }
}
