using KromicStore.Domain.StoreOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations.StoreOperations;

public sealed class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.ToTable("return_requests");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired();
        
        builder.Property(x => x.RequestedOnUtc).HasColumnName("requested_on_utc").IsRequired();
        builder.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomerNotes).HasColumnName("customer_notes").HasMaxLength(500);
        builder.Property(x => x.ItemCount).HasColumnName("item_count").IsRequired();
        builder.Property(x => x.ReturnAmount).HasColumnName("return_amount").HasPrecision(10, 2).IsRequired();
        
        builder.Property(x => x.ApprovedOnUtc).HasColumnName("approved_on_utc");
        builder.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(256);
        builder.Property(x => x.RejectedOnUtc).HasColumnName("rejected_on_utc");
        builder.Property(x => x.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
        builder.Property(x => x.ReceivedOnUtc).HasColumnName("received_on_utc");
        builder.Property(x => x.ReceivedNotes).HasColumnName("received_notes").HasMaxLength(500);
        builder.Property(x => x.CompletedOnUtc).HasColumnName("completed_on_utc");
        
        builder.Property(x => x.ReturnShippingLabel).HasColumnName("return_shipping_label").HasMaxLength(200);
        builder.Property(x => x.ReturnTrackingNumber).HasColumnName("return_tracking_number").HasMaxLength(100);
        builder.Property(x => x.ReturnShippedOnUtc).HasColumnName("return_shipped_on_utc");
        
        builder.Property(x => x.CreatedOnUtc).HasColumnName("created_on_utc").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnName("modified_on_utc").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(x => x.DeletedOnUtc).HasColumnName("deleted_on_utc");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(256);
        
        builder.HasIndex(x => new { x.TenantId, x.OrderId }).HasDatabaseName("idx_return_requests_order");
        builder.HasIndex(x => new { x.TenantId, x.CustomerId }).HasDatabaseName("idx_return_requests_customer");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_return_requests_status");
        builder.HasIndex(x => x.RequestedOnUtc).HasDatabaseName("idx_return_requests_requested");
    }
}
