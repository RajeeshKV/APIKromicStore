using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class ProductCollectionConfiguration : IEntityTypeConfiguration<ProductCollection>
{
    public void Configure(EntityTypeBuilder<ProductCollection> builder)
    {
        builder.ToTable("ProductCollections");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.ModifiedAtUtc)
            .IsRequired();

        builder.Property(c => c.ModifiedBy)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedOnUtc);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Collection Mappings (owned collection)
        builder.OwnsMany(c => c.ProductMappings, mapping =>
        {
            mapping.ToTable("ProductCollectionMappings");
            mapping.WithOwner().HasForeignKey("CollectionId");
            mapping.HasKey("Id");
            mapping.Property(m => m.ProductId).IsRequired();
            mapping.Property(m => m.DisplayOrder).HasDefaultValue(0);
            
            // Foreign key to Product table
            mapping.HasOne<Product>()
                .WithMany()
                .HasForeignKey("ProductId")
                .OnDelete(DeleteBehavior.Cascade);

            mapping.HasIndex(m => new { m.CollectionId, m.ProductId })
                .IsUnique();
        });

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.Name })
            .IsUnique()
            .HasDatabaseName("UX_Collection_Tenant_Name")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(c => new { c.TenantId, c.Status })
            .HasDatabaseName("IX_Collection_Tenant_Status");

        builder.HasIndex(c => c.IsDeleted)
            .HasDatabaseName("IX_Collection_IsDeleted");
    }
}
