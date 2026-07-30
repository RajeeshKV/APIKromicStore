using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class CheckoutItemConfiguration : IEntityTypeConfiguration<CheckoutItem>
{
    public void Configure(EntityTypeBuilder<CheckoutItem> builder)
    {
        builder.ToTable("CheckoutItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .ValueGeneratedNever();

        builder.Property(ci => ci.CheckoutSessionId)
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

        // Indexes
        builder.HasIndex(ci => ci.CheckoutSessionId)
            .HasDatabaseName("IX_CheckoutItem_CheckoutSessionId");

        builder.HasIndex(ci => ci.ProductId)
            .HasDatabaseName("IX_CheckoutItem_ProductId");
    }
}
