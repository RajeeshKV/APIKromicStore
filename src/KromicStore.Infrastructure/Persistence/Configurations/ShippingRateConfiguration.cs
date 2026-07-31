using KromicStore.Domain.Shipping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public class ShippingRateConfiguration : IEntityTypeConfiguration<ShippingRate>
{
    public void Configure(EntityTypeBuilder<ShippingRate> builder)
    {
        builder.ToTable("ShippingRates");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Id)
            .ValueGeneratedNever();
        
        builder.Property(r => r.TenantId)
            .IsRequired();
        
        builder.Property(r => r.ShippingMethodId)
            .IsRequired();
        
        builder.Property(r => r.MinWeight)
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(r => r.MaxWeight)
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(r => r.MinOrderValue)
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(r => r.MaxOrderValue)
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(r => r.Cost)
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(r => r.IsWeightBased)
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
        
        // Relationships
        builder.HasOne<ShippingMethod>()
            .WithMany()
            .HasForeignKey(r => r.ShippingMethodId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indices
        builder.HasIndex(r => new { r.TenantId, r.ShippingMethodId });
        builder.HasIndex(r => new { r.TenantId, r.IsActive });
        builder.HasIndex(r => new { r.IsWeightBased, r.MinWeight, r.MaxWeight });
        builder.HasIndex(r => new { r.IsWeightBased, r.MinOrderValue, r.MaxOrderValue });
    }
}
