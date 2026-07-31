using KromicStore.Domain.Email.Entities;

namespace KromicStore.Infrastructure.Services.Email;

/// <summary>
/// Abstraction for email service providers (Brevo, SendGrid, etc.).
/// Enables vendor-agnostic email delivery with template support.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email using a template with variable substitution.
    /// </summary>
    Task<EmailSendResult> SendTemplateEmailAsync(
        Guid tenantId,
        string recipientEmail,
        string recipientName,
        string templateId,
        Dictionary<string, string> variables,
        Dictionary<string, string>? customHeaders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a raw email without template.
    /// </summary>
    Task<EmailSendResult> SendRawEmailAsync(
        Guid tenantId,
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        Dictionary<string, string>? customHeaders = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send bulk emails to multiple recipients.
    /// </summary>
    Task<BulkEmailSendResult> SendBulkTemplateEmailAsync(
        Guid tenantId,
        List<BulkEmailRecipient> recipients,
        string templateId,
        Dictionary<string, string>? commonVariables = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify webhook signature from email provider.
    /// </summary>
    bool VerifyWebhookSignature(string payload, string signature);

    /// <summary>
    /// Health check - verify service connectivity.
    /// </summary>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of email send operation.
/// </summary>
public class EmailSendResult
{
    public bool Success { get; set; }
    public string? ExternalMessageId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Bulk email send result.
/// </summary>
public class BulkEmailSendResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkEmailItemResult> Results { get; set; } = [];
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual result in bulk email send.
/// </summary>
public class BulkEmailItemResult
{
    public string RecipientEmail { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ExternalMessageId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Recipient for bulk email operations.
/// </summary>
public class BulkEmailRecipient
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
}
