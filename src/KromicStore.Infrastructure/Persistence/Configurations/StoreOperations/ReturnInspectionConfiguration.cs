using KromicStore.Domain.StoreOperations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations.StoreOperations;

public sealed class ReturnInspectionConfiguration : IEntityTypeConfiguration<ReturnInspection>
{
    public void Configure(EntityTypeBuilder<ReturnInspection> builder)
    {
        builder.ToTable("return_inspections");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.ReturnRequestId).HasColumnName("return_request_id").IsRequired();
        builder.Property(x => x.InspectorNotes).HasColumnName("inspector_notes").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Result).HasColumnName("result").IsRequired();
        builder.Property(x => x.InspectedOnUtc).HasColumnName("inspected_on_utc").IsRequired();
        builder.Property(x => x.InspectedBy).HasColumnName("inspected_by").HasMaxLength(256).IsRequired();
        
        builder.Property(x => x.IsRestockable).HasColumnName("is_restockable").IsRequired();
        builder.Property(x => x.RestockableValue).HasColumnName("restockable_value").HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.WasteValue).HasColumnName("waste_value").HasPrecision(10, 2).IsRequired();
        
        builder.Property(x => x.CreatedOnUtc).HasColumnName("created_on_utc").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnName("modified_on_utc").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        
        builder.HasIndex(x => new { x.TenantId, x.ReturnRequestId }).IsUnique().HasDatabaseName("idx_return_inspections_return_request");
        builder.HasIndex(x => x.Result).HasDatabaseName("idx_return_inspections_result");
        builder.HasIndex(x => x.InspectedOnUtc).HasDatabaseName("idx_return_inspections_inspected");
    }
}
