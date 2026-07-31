using KromicStore.Domain.Taxes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public class TaxRegionConfiguration : IEntityTypeConfiguration<TaxRegion>
{
    public void Configure(EntityTypeBuilder<TaxRegion> builder)
    {
        builder.ToTable("TaxRegions");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .ValueGeneratedNever();
        
        builder.Property(r => r.TenantId)
            .IsRequired();
        
        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(r => r.CountryCode)
            .HasMaxLength(2)
            .IsRequired();
        
        builder.Property(r => r.StateCode)
            .HasMaxLength(3);
        
        builder.Property(r => r.IsTaxInclusive)
            .IsRequired();
        
        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(r => r.CreatedOnUtc)
            .IsRequired();
        
        builder.Property(r => r.ModifiedOnUtc)
            .IsRequired();
        
        builder.Property(r => r.CreatedBy)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(500);
        
        builder.Property(r => r.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(r => r.DeletedOnUtc);
        
        builder.Property(r => r.DeletedBy)
            .HasMaxLength(500);
        
        // Relationships
        builder.HasMany<TaxRule>()
            .WithOne()
            .HasForeignKey(tr => tr.TaxRegionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indices
        builder.HasIndex(r => new { r.TenantId, r.CountryCode, r.StateCode });
        builder.HasIndex(r => new { r.TenantId, r.IsActive });
        builder.HasIndex(r => new { r.TenantId, r.IsDeleted });
        builder.HasIndex(r => r.CreatedOnUtc);
        
        // Query filter for soft delete
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
