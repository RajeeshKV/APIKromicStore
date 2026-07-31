using KromicStore.Domain.Shipping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.ToTable("ShippingMethods");
        
        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.Id)
            .ValueGeneratedNever();
        
        builder.Property(m => m.TenantId)
            .IsRequired();
        
        builder.Property(m => m.ShippingZoneId)
            .IsRequired();
        
        builder.Property(m => m.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(m => m.Description)
            .HasMaxLength(1000);
        
        builder.Property(m => m.EstimatedDaysMin)
            .IsRequired();
        
        builder.Property(m => m.EstimatedDaysMax)
            .IsRequired();
        
        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(m => m.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(m => m.CreatedOnUtc)
            .IsRequired();
        
        builder.Property(m => m.ModifiedOnUtc)
            .IsRequired();
        
        builder.Property(m => m.CreatedBy)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(m => m.ModifiedBy)
            .HasMaxLength(500);
        
        // Relationships
        builder.HasOne<ShippingZone>()
            .WithMany()
            .HasForeignKey(m => m.ShippingZoneId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany<ShippingRate>()
            .WithOne()
            .HasForeignKey(r => r.ShippingMethodId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indices
        builder.HasIndex(m => new { m.TenantId, m.ShippingZoneId });
        builder.HasIndex(m => new { m.TenantId, m.IsActive });
        builder.HasIndex(m => new { m.TenantId, m.DisplayOrder });
        builder.HasIndex(m => m.CreatedOnUtc);
    }
}
