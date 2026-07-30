using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .ValueGeneratedNever();

        builder.Property(ci => ci.CartId)
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .IsRequired();

        builder.Property(ci => ci.ProductVariantId)
            .IsRequired(false);

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ci => ci.AddedOnUtc)
            .IsRequired();

        // Indexes
        builder.HasIndex(ci => ci.CartId)
            .HasDatabaseName("IX_CartItem_CartId");

        builder.HasIndex(ci => ci.ProductId)
            .HasDatabaseName("IX_CartItem_ProductId");

        builder.HasIndex(ci => new { ci.CartId, ci.ProductId, ci.ProductVariantId })
            .HasDatabaseName("UX_CartItem_Cart_Product_Variant")
            .IsUnique(true);
    }
}
