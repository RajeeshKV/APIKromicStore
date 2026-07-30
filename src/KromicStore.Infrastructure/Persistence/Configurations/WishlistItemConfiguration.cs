using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(wi => wi.Id);

        builder.Property(wi => wi.Id)
            .ValueGeneratedNever();

        builder.Property(wi => wi.WishlistId)
            .IsRequired();

        builder.Property(wi => wi.ProductId)
            .IsRequired();

        builder.Property(wi => wi.AddedOnUtc)
            .IsRequired();

        // Indexes
        builder.HasIndex(wi => wi.WishlistId)
            .HasDatabaseName("IX_WishlistItem_WishlistId");

        builder.HasIndex(wi => wi.ProductId)
            .HasDatabaseName("IX_WishlistItem_ProductId");

        builder.HasIndex(wi => new { wi.WishlistId, wi.ProductId })
            .HasDatabaseName("UX_WishlistItem_Wishlist_Product")
            .IsUnique(true);
    }
}
