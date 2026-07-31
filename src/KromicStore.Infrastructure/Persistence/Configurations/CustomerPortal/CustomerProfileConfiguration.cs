using KromicStore.Domain.CustomerPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations.CustomerPortal;

public sealed class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("customer_profiles");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();
        
        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();
        
        builder.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);
        
        builder.Property(x => x.DateOfBirth)
            .HasColumnName("date_of_birth");
        
        builder.Property(x => x.NewsletterOptIn)
            .HasColumnName("newsletter_opt_in")
            .IsRequired();
        
        builder.Property(x => x.NotificationPreferences)
            .HasColumnName("notification_preferences")
            .HasColumnType("jsonb");
        
        builder.Property(x => x.LastLoginUtc)
            .HasColumnName("last_login_utc");
        
        builder.Property(x => x.LoginCount)
            .HasColumnName("login_count")
            .IsRequired();
        
        // Auditing
        builder.Property(x => x.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();
        
        builder.Property(x => x.ModifiedOnUtc)
            .HasColumnName("modified_on_utc")
            .IsRequired();
        
        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(x => x.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(256);
        
        // Soft delete
        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();
        
        builder.Property(x => x.DeletedOnUtc)
            .HasColumnName("deleted_on_utc");
        
        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasMaxLength(256);
        
        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.CustomerId })
            .IsUnique()
            .HasDatabaseName("idx_customer_profiles_tenant_customer");
        
        builder.HasIndex(x => x.CreatedOnUtc)
            .HasDatabaseName("idx_customer_profiles_created");
    }
}
