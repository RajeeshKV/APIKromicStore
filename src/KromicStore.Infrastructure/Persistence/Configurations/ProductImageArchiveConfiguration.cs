using KromicStore.Domain.Media.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ProductImageArchive entity.
/// </summary>
public class ProductImageArchiveConfiguration : IEntityTypeConfiguration<ProductImageArchive>
{
    public void Configure(EntityTypeBuilder<ProductImageArchive> builder)
    {
        builder.ToTable("product_image_archive", "public");

        // Primary key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.TenantId)
            .IsRequired();

        builder.Property(a => a.ProductId)
            .IsRequired();

        builder.Property(a => a.PublicId)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.Url)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(a => a.SecureUrl)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(a => a.Width)
            .IsRequired();

        builder.Property(a => a.Height)
            .IsRequired();

        builder.Property(a => a.Format)
            .HasMaxLength(50);

        builder.Property(a => a.FileSizeBytes)
            .IsRequired();

        builder.Property(a => a.RestoredOnUtc);

        // Audit fields
        builder.Property(a => a.CreatedOnUtc)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.ModifiedOnUtc);

        builder.Property(a => a.ModifiedBy)
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(a => new { a.TenantId, a.ProductId })
            .HasDatabaseName("ix_product_image_archive_tenant_product");

        builder.HasIndex(a => a.PublicId)
            .HasDatabaseName("ix_product_image_archive_public_id");

        builder.HasIndex(a => a.CreatedOnUtc)
            .HasDatabaseName("ix_product_image_archive_created");

        builder.HasIndex(a => a.RestoredOnUtc)
            .HasDatabaseName("ix_product_image_archive_restored");
    }
}
