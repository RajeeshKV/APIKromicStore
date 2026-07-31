using KromicStore.Domain.Promotions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .ValueGeneratedNever();
        
        builder.Property(c => c.TenantId)
            .IsRequired();
        
        builder.Property(c => c.Code)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(c => c.Description)
            .HasMaxLength(500);
        
        builder.Property(c => c.DiscountId)
            .IsRequired();
        
        builder.Property(c => c.MaxUsageCount);
        
        builder.Property(c => c.MaxUsagePerCustomer);
        
        builder.Property(c => c.CurrentUsageCount)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(c => c.ValidFromUtc)
            .IsRequired();
        
        builder.Property(c => c.ValidToUtc)
            .IsRequired();
        
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(c => c.MinimumOrderValue)
            .HasPrecision(10, 2);
        
        builder.Property(c => c.ApplicableCategories)
            .HasMaxLength(2000);
        
        builder.Property(c => c.CreatedOnUtc)
            .IsRequired();
        
        builder.Property(c => c.ModifiedOnUtc)
            .IsRequired();
        
        builder.Property(c => c.CreatedBy)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(c => c.ModifiedBy)
            .HasMaxLength(500);
        
        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(c => c.DeletedOnUtc);
        
        builder.Property(c => c.DeletedBy)
            .HasMaxLength(500);
        
        // Indices
        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => new { c.TenantId, c.Code });
        builder.HasIndex(c => new { c.TenantId, c.IsActive });
        builder.HasIndex(c => new { c.TenantId, c.ValidFromUtc, c.ValidToUtc });
        builder.HasIndex(c => new { c.TenantId, c.IsDeleted });
        builder.HasIndex(c => c.CreatedOnUtc);
        
        // Query filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
