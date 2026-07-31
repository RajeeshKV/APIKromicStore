using KromicStore.Domain.Promotions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .ValueGeneratedNever();
        
        builder.Property(c => c.TenantId)
            .IsRequired();
        
        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(c => c.Description)
            .HasMaxLength(1000);
        
        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(c => c.ValidFromUtc)
            .IsRequired();
        
        builder.Property(c => c.ValidToUtc)
            .IsRequired();
        
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
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
        
        // DiscountIds as JSON array
        builder.Property(c => c.DiscountIds)
            .HasColumnName("DiscountIds")
            .HasConversion(
                d => JoinDiscountIds(d),
                d => SplitDiscountIds(d)
            );
        
        // Indices
        builder.HasIndex(c => new { c.TenantId, c.IsActive });
        builder.HasIndex(c => new { c.TenantId, c.DisplayOrder });
        builder.HasIndex(c => new { c.TenantId, c.ValidFromUtc, c.ValidToUtc });
        builder.HasIndex(c => new { c.TenantId, c.IsDeleted });
        builder.HasIndex(c => c.CreatedOnUtc);
        
        // Query filter for soft delete
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
    
    private static string JoinDiscountIds(IReadOnlyList<Guid> discountIds)
    {
        return string.Join(",", discountIds);
    }
    
    private static List<Guid> SplitDiscountIds(string discountIds)
    {
        return string.IsNullOrEmpty(discountIds) ? new List<Guid>() : discountIds.Split(",").Select(Guid.Parse).ToList();
    }
}
