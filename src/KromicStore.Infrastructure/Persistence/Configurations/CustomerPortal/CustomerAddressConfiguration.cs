using KromicStore.Domain.CustomerPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations.CustomerPortal;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("customer_addresses");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(x => x.Label).HasColumnName("label").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Street).HasColumnName("street").HasMaxLength(150).IsRequired();
        builder.Property(x => x.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        builder.Property(x => x.StateCode).HasColumnName("state_code").HasMaxLength(10).IsRequired();
        builder.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(2).IsRequired();
        builder.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
        builder.Property(x => x.IsShippingAddress).HasColumnName("is_shipping_address").IsRequired();
        builder.Property(x => x.IsBillingAddress).HasColumnName("is_billing_address").IsRequired();
        builder.Property(x => x.IsDefaultShipping).HasColumnName("is_default_shipping").IsRequired();
        builder.Property(x => x.IsDefaultBilling).HasColumnName("is_default_billing").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        
        builder.Property(x => x.CreatedOnUtc).HasColumnName("created_on_utc").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnName("modified_on_utc").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").IsRequired();
        builder.Property(x => x.DeletedOnUtc).HasColumnName("deleted_on_utc");
        builder.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(256);
        
        builder.HasIndex(x => new { x.TenantId, x.CustomerId })
            .HasDatabaseName("idx_customer_addresses_tenant_customer");
        builder.HasIndex(x => x.CreatedOnUtc)
            .HasDatabaseName("idx_customer_addresses_created");
    }
}
