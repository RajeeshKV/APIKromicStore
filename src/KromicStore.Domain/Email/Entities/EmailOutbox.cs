using KromicStore.Domain.Common;

namespace KromicStore.Domain.Email.Entities;

/// <summary>
/// Email Outbox implements the Outbox pattern for reliable email delivery.
/// Ensures emails are persisted before sending, enabling retry and auditing.
/// </summary>
public class EmailOutbox : AuditableEntity, ITenantEntity
{
    private EmailOutbox() { }

    private EmailOutbox(Guid id) : base(id) { }

    public Guid TenantId { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string TemplateType { get; private set; } = string.Empty;
    public long TemplateId { get; private set; }
    public Dictionary<string, string>? TemplateVariables { get; private set; }
    public string? Subject { get; private set; }
    public string? HtmlBody { get; private set; }
    public string? PlainTextBody { get; private set; }
    public Dictionary<string, string>? CustomHeaders { get; private set; }
    public EmailOutboxStatus Status { get; private set; } = EmailOutboxStatus.Pending;
    public DateTime? ProcessedOnUtc { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public DateTime? NextRetryAtUtc { get; private set; }
    public string? ExternalMessageId { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Create a new email outbox entry for template-based email.
    /// </summary>
    public static EmailOutbox CreateTemplate(
        Guid tenantId,
        string recipientEmail,
        string recipientName,
        string templateType,
        long templateId,
        Dictionary<string, string>? variables = null,
        Dictionary<string, string>? customHeaders = null,
        string createdBy = "system")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail, nameof(recipientEmail));
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName, nameof(recipientName));
        ArgumentException.ThrowIfNullOrWhiteSpace(templateType, nameof(templateType));

        var email = new EmailOutbox(Guid.NewGuid())
        {
            TenantId = tenantId,
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            TemplateType = templateType,
            TemplateId = templateId,
            TemplateVariables = variables,
            CustomHeaders = customHeaders,
            Status = EmailOutboxStatus.Pending,
            AttemptCount = 0,
            MaxAttempts = 3
        };
        
        email.MarkCreated(DateTime.UtcNow, createdBy);
        return email;
    }

    /// <summary>
    /// Create a new email outbox entry for raw email.
    /// </summary>
    public static EmailOutbox CreateRaw(
        Guid tenantId,
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        Dictionary<string, string>? customHeaders = null,
        string createdBy = "system")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail, nameof(recipientEmail));
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName, nameof(recipientName));
        ArgumentException.ThrowIfNullOrWhiteSpace(subject, nameof(subject));
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlBody, nameof(htmlBody));

        var email = new EmailOutbox(Guid.NewGuid())
        {
            TenantId = tenantId,
            RecipientEmail = recipientEmail,
            RecipientName = recipientName,
            TemplateType = "RawEmail",
            TemplateId = 0,
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainTextBody,
            CustomHeaders = customHeaders,
            Status = EmailOutboxStatus.Pending,
            AttemptCount = 0,
            MaxAttempts = 3
        };
        
        email.MarkCreated(DateTime.UtcNow, createdBy);
        return email;
    }

    /// <summary>
    /// Mark email as sent successfully.
    /// </summary>
    public void MarkAsSent(string externalMessageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalMessageId, nameof(externalMessageId));
        
        Status = EmailOutboxStatus.Sent;
        ProcessedOnUtc = DateTime.UtcNow;
        ExternalMessageId = externalMessageId;
        ErrorCode = null;
        ErrorMessage = null;
        MarkModified(DateTime.UtcNow, "system");
    }

    /// <summary>
    /// Mark email as failed and schedule retry if attempts remain.
    /// </summary>
    public void MarkAsFailed(string errorCode, string errorMessage, int retryDelaySeconds = 60)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode, nameof(errorCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage, nameof(errorMessage));

        AttemptCount++;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;

        if (AttemptCount >= MaxAttempts)
        {
            Status = EmailOutboxStatus.Failed;
            FailureReason = $"Max attempts ({MaxAttempts}) exceeded";
        }
        else
        {
            Status = EmailOutboxStatus.Pending;
            NextRetryAtUtc = DateTime.UtcNow.AddSeconds(retryDelaySeconds * AttemptCount);
        }
        
        MarkModified(DateTime.UtcNow, "system");
    }

    /// <summary>
    /// Mark email as processing.
    /// </summary>
    public void MarkAsProcessing()
    {
        Status = EmailOutboxStatus.Processing;
        MarkModified(DateTime.UtcNow, "system");
    }
}

/// <summary>
/// Email outbox status enum.
/// </summary>
public enum EmailOutboxStatus
{
    Pending = 0,
    Processing = 1,
    Sent = 2,
    Failed = 3,
    Abandoned = 4
}
