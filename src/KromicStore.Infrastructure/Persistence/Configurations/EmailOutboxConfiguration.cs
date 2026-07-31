using System.Text.Json;
using KromicStore.Domain.Email.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for EmailOutbox entity.
/// </summary>
public class EmailOutboxConfiguration : IEntityTypeConfiguration<EmailOutbox>
{
    public void Configure(EntityTypeBuilder<EmailOutbox> builder)
    {
        builder.ToTable("email_outbox", "public");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.TenantId)
            .IsRequired();

        builder.Property(e => e.RecipientEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.RecipientName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.TemplateType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.TemplateId)
            .IsRequired();

        // Dictionary properties - configure as ignored for InMemory, mapped for PostgreSQL
        // Tests use InMemory which doesn't support complex types
        builder.Property(e => e.TemplateVariables)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null))
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(e => e.CustomHeaders)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null))
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(e => e.Subject)
            .HasMaxLength(500);

        builder.Property(e => e.HtmlBody)
            .HasColumnType("text");

        builder.Property(e => e.PlainTextBody)
            .HasColumnType("text");

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ProcessedOnUtc);

        builder.Property(e => e.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.MaxAttempts)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(e => e.NextRetryAtUtc);

        builder.Property(e => e.ExternalMessageId)
            .HasMaxLength(255);

        builder.Property(e => e.ErrorCode)
            .HasMaxLength(100);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(e => e.FailureReason)
            .HasMaxLength(1000);

        // Audit fields
        builder.Property(e => e.CreatedOnUtc)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.ModifiedOnUtc);

        builder.Property(e => e.ModifiedBy)
            .HasMaxLength(255);

        // Indexes for querying
        builder.HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("ix_email_outbox_tenant_status");

        builder.HasIndex(e => new { e.Status, e.NextRetryAtUtc })
            .HasDatabaseName("ix_email_outbox_status_retry");

        builder.HasIndex(e => e.CreatedOnUtc)
            .HasDatabaseName("ix_email_outbox_created");

        builder.HasIndex(e => e.ProcessedOnUtc)
            .HasDatabaseName("ix_email_outbox_processed");
    }
}
