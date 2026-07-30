using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the TenantSettings entity.
/// </summary>
public sealed class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.HasKey(ts => ts.Id);

        builder.Property(ts => ts.TenantId)
            .IsRequired();

        builder.Property(ts => ts.LogoUrl)
            .HasMaxLength(512);

        builder.Property(ts => ts.FaviconUrl)
            .HasMaxLength(512);

        builder.Property(ts => ts.PrimaryColor)
            .HasMaxLength(7);

        builder.Property(ts => ts.SecondaryColor)
            .HasMaxLength(7);

        builder.Property(ts => ts.ContactEmail)
            .HasMaxLength(256);

        builder.Property(ts => ts.ContactPhone)
            .HasMaxLength(20);

        builder.Property(ts => ts.Address)
            .HasMaxLength(512);

        builder.Property(ts => ts.City)
            .HasMaxLength(100);

        builder.Property(ts => ts.State)
            .HasMaxLength(100);

        builder.Property(ts => ts.Country)
            .HasMaxLength(100);

        builder.Property(ts => ts.PostalCode)
            .HasMaxLength(20);

        builder.Property(ts => ts.RazorpayKeyId)
            .HasMaxLength(256);

        builder.Property(ts => ts.RazorpayKeySecret)
            .HasMaxLength(256);

        // Audit fields
        builder.Property(ts => ts.CreatedOnUtc)
            .IsRequired();

        builder.Property(ts => ts.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ts => ts.ModifiedOnUtc);

        builder.Property(ts => ts.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(ts => ts.DeletedOnUtc);

        builder.Property(ts => ts.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(ts => ts.TenantId)
            .IsUnique();

        builder.HasIndex(ts => ts.IsDeleted);

        builder.ToTable("TenantSettings", "public");
    }
}
