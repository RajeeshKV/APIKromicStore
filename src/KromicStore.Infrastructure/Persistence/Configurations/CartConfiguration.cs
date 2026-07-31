using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .IsRequired();

        builder.Property(c => c.CustomerId)
            .IsRequired(false);

        builder.Property(c => c.AnonymousSessionId)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(c => c.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.LastActivityOnUtc)
            .IsRequired();

        builder.Property(c => c.ExpiresOnUtc)
            .IsRequired();

        // Auditing
        builder.Property(c => c.CreatedOnUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.ModifiedOnUtc)
            .HasColumnName("ModifiedAtUtc")
            .IsRequired();

        builder.Property(c => c.ModifiedBy)
            .HasMaxLength(255)
            .IsRequired();

        // Soft delete
        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedOnUtc)
            .IsRequired(false);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(255)
            .IsRequired(false);

        // Relationships
        builder.HasMany<CartItem>()
            .WithOne()
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("IX_Cart_TenantId");

        builder.HasIndex(c => c.CustomerId)
            .HasDatabaseName("IX_Cart_CustomerId")
            .IsUnique(false);

        builder.HasIndex(c => c.AnonymousSessionId)
            .HasDatabaseName("IX_Cart_AnonymousSessionId")
            .IsUnique(false);

        builder.HasIndex(c => c.ExpiresOnUtc)
            .HasDatabaseName("IX_Cart_ExpiresOnUtc");

        builder.HasIndex(c => new { c.TenantId, c.CustomerId })
            .HasDatabaseName("UX_Cart_Tenant_Customer")
            .IsUnique(false);

        builder.HasIndex(c => new { c.TenantId, c.AnonymousSessionId })
            .HasDatabaseName("UX_Cart_Tenant_Session")
            .IsUnique(false);
    }
}
