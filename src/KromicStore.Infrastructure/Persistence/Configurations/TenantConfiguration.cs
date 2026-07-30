using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Tenant entity.
/// </summary>
public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.StoreName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.OwnerUserId);

        // Audit fields
        builder.Property(t => t.CreatedOnUtc)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.ModifiedOnUtc);

        builder.Property(t => t.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(t => t.DeletedOnUtc);

        builder.Property(t => t.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.HasIndex(t => t.Status);

        builder.HasIndex(t => t.IsDeleted);

        // Relationships
        builder.HasMany(t => t.Domains)
            .WithOne()
            .HasForeignKey(td => td.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Tenants", "public");
    }
}
