using KromicStore.Domain.Common;

namespace KromicStore.Domain.Email.Entities;

/// <summary>
/// Represents an email that has been sent or is in the queue.
/// Tenant-scoped entity for email delivery tracking.
/// </summary>
public class Email : TenantEntity, IAuditable, ISoftDeletable
{
    public string RecipientEmail { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string HtmlBody { get; private set; } = string.Empty;
    public EmailTemplate Template { get; private set; }
    public Dictionary<string, string> TemplateVariables { get; private set; } = new();
    public EmailStatus Status { get; private set; } = EmailStatus.Pending;
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? ExternalMessageId { get; private set; }

    private Email() { }

    private Email(Guid id, Guid tenantId) : base(id, tenantId) { }

    /// <summary>
    /// Creates a new email entity from template variables.
    /// </summary>
    public static Email Create(
        Guid tenantId,
        string recipientEmail,
        string recipientName,
        EmailTemplate template,
        Dictionary<string, string> templateVariables,
        string subject,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email is required.", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(recipientName))
            throw new ArgumentException("Recipient name is required.", nameof(recipientName));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by is required.", nameof(createdBy));

        var email = new Email(Guid.NewGuid(), tenantId)
        {
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            Template = template,
            TemplateVariables = templateVariables ?? new(),
            Subject = subject,
            Status = EmailStatus.Pending,
            RetryCount = 0
        };

        email.MarkCreated(DateTime.UtcNow, createdBy);
        return email;
    }

    /// <summary>
    /// Marks the email as sent successfully.
    /// </summary>
    public void MarkAsSent(string externalMessageId, string actor)
    {
        if (string.IsNullOrWhiteSpace(externalMessageId))
            throw new ArgumentException("External message ID is required.", nameof(externalMessageId));

        Status = EmailStatus.Sent;
        SentAt = DateTime.UtcNow;
        ExternalMessageId = externalMessageId;
        ErrorMessage = null;
        MarkModified(DateTime.UtcNow, actor);
    }

    /// <summary>
    /// Marks the email as failed with an error message.
    /// </summary>
    public void MarkAsFailed(string errorMessage, string actor)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message is required.", nameof(errorMessage));

        Status = EmailStatus.Failed;
        ErrorMessage = errorMessage;
        RetryCount++;
        MarkModified(DateTime.UtcNow, actor);
    }

    /// <summary>
    /// Marks the email as queued for sending.
    /// </summary>
    public void MarkAsQueued(string actor)
    {
        Status = EmailStatus.Queued;
        MarkModified(DateTime.UtcNow, actor);
    }

    /// <summary>
    /// Soft delete the email record.
    /// </summary>
    public void MarkAsDeleted(string actor)
    {
        SoftDelete(DateTime.UtcNow, actor);
    }

    /// <summary>
    /// Restore a soft-deleted email record.
    /// </summary>
    public void Restore(string actor)
    {
        Restore(DateTime.UtcNow, actor);
    }
}

/// <summary>
/// Email status enumeration.
/// </summary>
public enum EmailStatus
{
    Pending = 0,
    Queued = 1,
    Sent = 2,
    Failed = 3,
    Bounced = 4,
    Complained = 5
}

/// <summary>
/// Email template types.
/// </summary>
public enum EmailTemplate
{
    Welcome = 1,
    VerifyEmail = 2,
    ForgotPassword = 3,
    PasswordChanged = 4,
    TenantInvitation = 5,
    CustomerInvitation = 6,
    OrderConfirmation = 7,
    OrderCancelled = 8,
    ShipmentCreated = 9,
    ShipmentDelivered = 10,
    ReturnRequested = 11,
    RefundCompleted = 12,
    ContactUsAutoReply = 13,
    ContactUsInternal = 14
}
