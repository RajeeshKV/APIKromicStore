using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.Property(w => w.TenantId)
            .IsRequired();

        builder.Property(w => w.CustomerId)
            .IsRequired();

        // Auditing
        builder.Property(w => w.CreatedAtUtc)
            .IsRequired();

        builder.Property(w => w.CreatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(w => w.ModifiedAtUtc)
            .IsRequired();

        builder.Property(w => w.ModifiedBy)
            .HasMaxLength(255)
            .IsRequired();

        // Soft delete
        builder.Property(w => w.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(w => w.DeletedOnUtc)
            .IsRequired(false);

        builder.Property(w => w.DeletedBy)
            .HasMaxLength(255)
            .IsRequired(false);

        // Relationships
        builder.HasMany<WishlistItem>()
            .WithOne()
            .HasForeignKey(wi => wi.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => w.TenantId)
            .HasDatabaseName("IX_Wishlist_TenantId");

        builder.HasIndex(w => w.CustomerId)
            .HasDatabaseName("IX_Wishlist_CustomerId");

        builder.HasIndex(w => new { w.TenantId, w.CustomerId })
            .HasDatabaseName("UX_Wishlist_Tenant_Customer")
            .IsUnique(true);
    }
}
