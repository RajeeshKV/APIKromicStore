using KromicStore.Domain.Taxes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder.ToTable("TaxRules");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .ValueGeneratedNever();
        
        builder.Property(r => r.TenantId)
            .IsRequired();
        
        builder.Property(r => r.TaxRegionId)
            .IsRequired();
        
        builder.Property(r => r.ProductCategory)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(r => r.TaxRate)
            .HasPrecision(5, 4) // 0.0000 to 1.0000
            .IsRequired();
        
        builder.Property(r => r.Description)
            .HasMaxLength(1000);
        
        builder.Property(r => r.EffectiveFromUtc);
        
        builder.Property(r => r.EffectiveToUtc);
        
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
        
        // Relationships
        builder.HasOne<TaxRegion>()
            .WithMany()
            .HasForeignKey(r => r.TaxRegionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indices
        builder.HasIndex(r => new { r.TenantId, r.TaxRegionId });
        builder.HasIndex(r => new { r.TenantId, r.ProductCategory });
        builder.HasIndex(r => new { r.TenantId, r.IsActive });
        builder.HasIndex(r => new { r.EffectiveFromUtc, r.EffectiveToUtc });
        builder.HasIndex(r => r.CreatedOnUtc);
    }
}
