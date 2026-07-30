using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the TenantDomain entity.
/// </summary>
public sealed class TenantDomainConfiguration : IEntityTypeConfiguration<TenantDomain>
{
    public void Configure(EntityTypeBuilder<TenantDomain> builder)
    {
        builder.HasKey(td => td.Id);

        builder.Property(td => td.TenantId)
            .IsRequired();

        builder.Property(td => td.Subdomain)
            .HasMaxLength(256);

        builder.Property(td => td.CustomDomain)
            .HasMaxLength(256);

        builder.Property(td => td.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(td => td.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        // Audit fields
        builder.Property(td => td.CreatedOnUtc)
            .IsRequired();

        builder.Property(td => td.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(td => td.ModifiedOnUtc);

        builder.Property(td => td.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(td => td.DeletedOnUtc);

        builder.Property(td => td.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(td => td.Subdomain)
            .IsUnique();

        builder.HasIndex(td => td.CustomDomain)
            .IsUnique();

        builder.HasIndex(td => td.TenantId);

        builder.HasIndex(td => td.IsDeleted);

        builder.ToTable("TenantDomains", "public");
    }
}
