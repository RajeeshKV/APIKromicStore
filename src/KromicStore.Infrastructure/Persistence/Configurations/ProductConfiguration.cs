using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.ProductType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(p => p.CompareAtPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.Weight);
        builder.Property(p => p.Length);
        builder.Property(p => p.Width);
        builder.Property(p => p.Height);

        builder.Property(p => p.IsFeatured)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.TrackInventory)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.Taxable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.ModifiedAtUtc)
            .IsRequired();

        builder.Property(p => p.ModifiedBy)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.DeletedOnUtc);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product Images
        builder.OwnsMany(p => p.Images, image =>
        {
            image.ToTable("ProductImages");
            image.WithOwner().HasForeignKey("ProductId");
            image.HasKey("Id");
            image.Property(i => i.Url).IsRequired().HasMaxLength(500);
            image.Property(i => i.PublicId).IsRequired().HasMaxLength(500);
            image.Property(i => i.AltText).HasMaxLength(300);
            image.Property(i => i.DisplayOrder).HasDefaultValue(0);
            image.Property(i => i.IsPrimary).HasDefaultValue(false);
            image.Property(i => i.IsDeleted).HasDefaultValue(false);
            image.Property(i => i.DeletedOnUtc);
            image.Property(i => i.DeletedBy).HasMaxLength(500);
            image.HasIndex(i => new { i.ProductId, i.DisplayOrder });
        });

        // Product Variants
        builder.OwnsMany(p => p.Variants, variant =>
        {
            variant.ToTable("ProductVariants");
            variant.WithOwner().HasForeignKey("ProductId");
            variant.HasKey("Id");
            variant.Property(v => v.ProductId).IsRequired();
            variant.Property(v => v.Sku).IsRequired().HasMaxLength(50);
            variant.Property(v => v.Name).IsRequired().HasMaxLength(200);
            variant.Property(v => v.PriceAdjustment).HasPrecision(18, 2).HasDefaultValue(0m);
            variant.Property(v => v.StockQuantity).HasDefaultValue(0);
            variant.Property(v => v.IsActive).HasDefaultValue(true);
            
            // Ignore Attributes navigation - managed by domain logic, not persisted separately
            variant.Ignore(v => v.Attributes);

            variant.HasIndex(v => new { v.ProductId, v.IsActive });
        });

        // Product Attributes
        builder.OwnsMany(p => p.Attributes, attr =>
        {
            attr.ToTable("ProductAttributes");
            attr.WithOwner().HasForeignKey("ProductId");
            attr.HasKey("Id");
            attr.Property(a => a.AttributeName).IsRequired().HasMaxLength(100);
            attr.Property(a => a.AttributeValue).IsRequired().HasMaxLength(500);
            attr.Property(a => a.IsDeleted).HasDefaultValue(false);
            attr.Property(a => a.DeletedOnUtc);
            attr.Property(a => a.DeletedBy).HasMaxLength(500);
            attr.HasIndex(a => new { a.ProductId, a.AttributeName });
        });

        // Product Tags
        builder.OwnsMany(p => p.Tags, tag =>
        {
            tag.ToTable("ProductTags");
            tag.WithOwner().HasForeignKey("ProductId");
            tag.HasKey("Id");
            tag.Property(t => t.Tag).IsRequired().HasMaxLength(50);
            tag.Property(t => t.IsDeleted).HasDefaultValue(false);
            tag.Property(t => t.DeletedOnUtc);
            tag.Property(t => t.DeletedBy).HasMaxLength(500);
            tag.HasIndex(t => new { t.ProductId, t.Tag });
        });

        // Inventory (owned entity - one-to-one)
        builder.OwnsOne(p => p.Inventory, inventory =>
        {
            inventory.ToTable("Inventory");
            inventory.WithOwner().HasForeignKey("ProductId");
            inventory.HasKey("Id");
            inventory.Property(i => i.ProductId).IsRequired();
            inventory.Property(i => i.AvailableQuantity).HasDefaultValue(0);
            inventory.Property(i => i.ReservedQuantity).HasDefaultValue(0);
            inventory.Property(i => i.ReorderLevel).HasDefaultValue(10);
            inventory.Property(i => i.LastAdjustedUtc).IsRequired();
            inventory.HasIndex(i => new { i.ProductId, i.AvailableQuantity });
        });

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.Sku })
            .IsUnique()
            .HasDatabaseName("UX_Product_Tenant_SKU")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(p => new { p.TenantId, p.Slug })
            .IsUnique()
            .HasDatabaseName("UX_Product_Tenant_Slug")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(p => new { p.TenantId, p.CategoryId })
            .HasDatabaseName("IX_Product_Tenant_Category");

        builder.HasIndex(p => new { p.TenantId, p.Status })
            .HasDatabaseName("IX_Product_Tenant_Status");

        builder.HasIndex(p => new { p.TenantId, p.IsFeatured })
            .HasDatabaseName("IX_Product_Tenant_Featured");

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Product_IsDeleted");
    }
}
