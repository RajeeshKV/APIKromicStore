using KromicStore.Domain.CustomerPortal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations.CustomerPortal;

public sealed class CustomerNotificationPreferenceConfiguration : IEntityTypeConfiguration<CustomerNotificationPreference>
{
    public void Configure(EntityTypeBuilder<CustomerNotificationPreference> builder)
    {
        builder.ToTable("customer_notification_preferences");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(x => x.NotificationType).HasColumnName("notification_type").IsRequired();
        
        builder.Property(x => x.EmailEnabled).HasColumnName("email_enabled").IsRequired();
        builder.Property(x => x.SMSEnabled).HasColumnName("sms_enabled").IsRequired();
        builder.Property(x => x.PushEnabled).HasColumnName("push_enabled").IsRequired();
        builder.Property(x => x.InAppEnabled).HasColumnName("in_app_enabled").IsRequired();
        
        builder.Property(x => x.Frequency).HasColumnName("frequency").IsRequired();
        
        builder.Property(x => x.CreatedOnUtc).HasColumnName("created_on_utc").IsRequired();
        builder.Property(x => x.ModifiedOnUtc).HasColumnName("modified_on_utc").IsRequired();
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(256).IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        
        builder.HasIndex(x => new { x.TenantId, x.CustomerId, x.NotificationType })
            .IsUnique()
            .HasDatabaseName("idx_notification_prefs_tenant_customer_type");
    }
}
