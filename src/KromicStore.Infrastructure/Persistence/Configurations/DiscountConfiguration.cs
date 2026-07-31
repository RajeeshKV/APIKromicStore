using KromicStore.Domain.Promotions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id)
            .ValueGeneratedNever();
        
        builder.Property(d => d.TenantId)
            .IsRequired();
        
        builder.Property(d => d.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(d => d.Description)
            .HasMaxLength(1000);
        
        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(d => d.FixedAmount)
            .HasPrecision(10, 2);
        
        builder.Property(d => d.PercentageAmount)
            .HasPrecision(5, 4);
        
        builder.Property(d => d.MaxDiscountAmount)
            .HasPrecision(10, 2);
        
        builder.Property(d => d.BuyProductId)
            .HasMaxLength(100);
        
        builder.Property(d => d.BuyQuantity);
        
        builder.Property(d => d.GetProductId)
            .HasMaxLength(100);
        
        builder.Property(d => d.GetQuantity);
        
        builder.Property(d => d.GetDiscount)
            .HasPrecision(5, 4);
        
        builder.Property(d => d.FreeShippingMinimum)
            .HasPrecision(10, 2);
        
        builder.Property(d => d.AppliesToWholeOrder)
            .IsRequired();
        
        builder.Property(d => d.ApplicableProductIds)
            .HasMaxLength(2000);
        
        builder.Property(d => d.ApplicableCategories)
            .HasMaxLength(2000);
        
        builder.Property(d => d.ValidFromUtc)
            .IsRequired();
        
        builder.Property(d => d.ValidToUtc)
            .IsRequired();
        
        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(d => d.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(d => d.MaxUsageCount);
        
        builder.Property(d => d.CurrentUsageCount)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(d => d.CreatedOnUtc)
            .IsRequired();
        
        builder.Property(d => d.ModifiedOnUtc)
            .IsRequired();
        
        builder.Property(d => d.CreatedBy)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(d => d.ModifiedBy)
            .HasMaxLength(500);
        
        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(d => d.DeletedOnUtc);
        
        builder.Property(d => d.DeletedBy)
            .HasMaxLength(500);
        
        // Indices
        builder.HasIndex(d => new { d.TenantId, d.Type });
        builder.HasIndex(d => new { d.TenantId, d.IsActive });
        builder.HasIndex(d => new { d.TenantId, d.DisplayOrder });
        builder.HasIndex(d => new { d.TenantId, d.ValidFromUtc, d.ValidToUtc });
        builder.HasIndex(d => new { d.TenantId, d.IsDeleted });
        builder.HasIndex(d => d.CreatedOnUtc);
        
        // Query filter for soft delete
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
