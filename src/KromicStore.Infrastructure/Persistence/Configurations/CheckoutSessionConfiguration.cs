using KromicStore.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

public sealed class CheckoutSessionConfiguration : IEntityTypeConfiguration<CheckoutSession>
{
    public void Configure(EntityTypeBuilder<CheckoutSession> builder)
    {
        builder.ToTable("CheckoutSessions");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.Id)
            .ValueGeneratedNever();

        builder.Property(cs => cs.TenantId)
            .IsRequired();

        builder.Property(cs => cs.CustomerId)
            .IsRequired();

        builder.Property(cs => cs.BillingAddressId)
            .IsRequired(false);

        builder.Property(cs => cs.ShippingAddressId)
            .IsRequired(false);

        builder.Property(cs => cs.ShippingMethod)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(cs => cs.PaymentMethod)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(cs => cs.Status)
            .IsRequired();

        builder.Property(cs => cs.CreatedOnUtc)
            .IsRequired();

        builder.Property(cs => cs.ExpiresOnUtc)
            .IsRequired(false);

        // Pricing
        builder.Property(cs => cs.SubTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(cs => cs.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(cs => cs.ShippingAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(cs => cs.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(cs => cs.GrandTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        // Coupon
        builder.Property(cs => cs.CouponCode)
            .HasMaxLength(50)
            .IsRequired(false);

        // Auditing
        builder.Property(cs => cs.ModifiedAtUtc)
            .IsRequired();

        builder.Property(cs => cs.CreatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(cs => cs.ModifiedBy)
            .HasMaxLength(255)
            .IsRequired();

        // Soft delete
        builder.Property(cs => cs.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cs => cs.DeletedOnUtc)
            .IsRequired(false);

        builder.Property(cs => cs.DeletedBy)
            .HasMaxLength(255)
            .IsRequired(false);

        // Relationships
        builder.HasMany<CheckoutItem>()
            .WithOne()
            .HasForeignKey(ci => ci.CheckoutSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cs => cs.TenantId)
            .HasDatabaseName("IX_CheckoutSession_TenantId");

        builder.HasIndex(cs => cs.CustomerId)
            .HasDatabaseName("IX_CheckoutSession_CustomerId");

        builder.HasIndex(cs => cs.Status)
            .HasDatabaseName("IX_CheckoutSession_Status");

        builder.HasIndex(cs => cs.ExpiresOnUtc)
            .HasDatabaseName("IX_CheckoutSession_ExpiresOnUtc");

        builder.HasIndex(cs => new { cs.TenantId, cs.CustomerId })
            .HasDatabaseName("IX_CheckoutSession_Tenant_Customer");
    }
}
