using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.IsVisible)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.MetaTitle)
            .HasMaxLength(100);

        builder.Property(c => c.MetaDescription)
            .HasMaxLength(200);

        builder.Property(c => c.CreatedOnUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.ModifiedOnUtc)
            .HasColumnName("ModifiedAtUtc")
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

        // Self-referencing relationship for parent category
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(c => new { c.TenantId, c.Slug })
            .IsUnique()
            .HasDatabaseName("UX_Category_Tenant_Slug")
            .HasFilter("\"IsDeleted\" = 0");

        builder.HasIndex(c => new { c.TenantId, c.ParentCategoryId })
            .HasDatabaseName("IX_Category_Tenant_Parent");

        builder.HasIndex(c => new { c.TenantId, c.Status })
            .HasDatabaseName("IX_Category_Tenant_Status");

        builder.HasIndex(c => new { c.TenantId, c.IsVisible })
            .HasDatabaseName("IX_Category_Tenant_Visible");

        builder.HasIndex(c => c.IsDeleted)
            .HasDatabaseName("IX_Category_IsDeleted");
    }
}
