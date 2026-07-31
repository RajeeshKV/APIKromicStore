using KromicStore.Domain.Shipping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.ToTable("ShippingZones");
        
        builder.HasKey(z => z.Id);
        
        builder.Property(z => z.Id)
            .ValueGeneratedNever();
        
        builder.Property(z => z.TenantId)
            .IsRequired();
        
        builder.Property(z => z.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(z => z.Description)
            .HasMaxLength(1000);
        
        builder.Property(z => z.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(z => z.CreatedOnUtc)
            .IsRequired();
        
        builder.Property(z => z.ModifiedOnUtc)
            .IsRequired();
        
        builder.Property(z => z.CreatedBy)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(z => z.ModifiedBy)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(z => z.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(z => z.DeletedOnUtc);
        
        builder.Property(z => z.DeletedBy)
            .HasMaxLength(500);
        
        // Countries as JSON array
        builder.Property(z => z.Countries)
            .HasColumnName("Countries")
            .HasConversion(
                c => JoinCountries(c),
                c => SplitCountries(c)
            );
        
        // Relationships
        builder.HasMany<ShippingMethod>()
            .WithOne()
            .HasForeignKey(m => m.ShippingZoneId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indices
        builder.HasIndex(z => new { z.TenantId, z.IsActive });
        builder.HasIndex(z => new { z.TenantId, z.IsDeleted });
        builder.HasIndex(z => z.CreatedOnUtc);
        
        // Query filter for soft delete
        builder.HasQueryFilter(z => !z.IsDeleted);
    }
    
    private static string JoinCountries(IReadOnlyList<string> countries)
    {
        return string.Join(",", countries);
    }
    
    private static List<string> SplitCountries(string countries)
    {
        return string.IsNullOrEmpty(countries) ? new List<string>() : countries.Split(",").ToList();
    }
}
