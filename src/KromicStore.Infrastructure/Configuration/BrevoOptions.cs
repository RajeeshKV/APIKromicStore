using System.ComponentModel.DataAnnotations;

namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// Brevo email service configuration.
/// Strongly typed configuration with validation.
/// </summary>
public class BrevoOptions
{
    public const string SectionName = "Brevo";

    [Required(ErrorMessage = "Brevo API key is required")]
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Brevo sender name is required")]
    public string SenderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Brevo sender email is required")]
    [EmailAddress(ErrorMessage = "Brevo sender email must be a valid email address")]
    public string SenderEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Brevo webhook secret is required")]
    public string WebhookSecret { get; set; } = string.Empty;

    [Url(ErrorMessage = "Brevo base URL must be a valid URL")]
    public string BaseUrl { get; set; } = "https://api.brevo.com/v3";

    public int RequestTimeoutSeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 3;

    public int InitialRetryDelayMilliseconds { get; set; } = 1000;

    public double RetryBackoffMultiplier { get; set; } = 2.0;

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Email template IDs mapped by template type.
    /// </summary>
    public Dictionary<string, long> TemplateIds { get; set; } = new()
    {
        ["Welcome"] = 0,
        ["VerifyEmail"] = 0,
        ["ForgotPassword"] = 0,
        ["PasswordChanged"] = 0,
        ["TenantInvitation"] = 0,
        ["CustomerInvitation"] = 0,
        ["OrderConfirmation"] = 0,
        ["OrderCancelled"] = 0,
        ["ShipmentCreated"] = 0,
        ["ShipmentDelivered"] = 0,
        ["ReturnRequested"] = 0,
        ["RefundCompleted"] = 0,
        ["ContactUsAutoReply"] = 0,
        ["ContactUsInternal"] = 0
    };

    /// <summary>
    /// Validate configuration.
    /// </summary>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
            return (false, "Brevo API key is required");

        if (ApiKey.Length < 20)
            return (false, "Brevo API key appears to be invalid (too short)");

        if (string.IsNullOrWhiteSpace(SenderName))
            return (false, "Brevo sender name is required");

        if (string.IsNullOrWhiteSpace(SenderEmail))
            return (false, "Brevo sender email is required");

        if (!SenderEmail.Contains("@"))
            return (false, "Brevo sender email must be a valid email address");

        if (string.IsNullOrWhiteSpace(WebhookSecret))
            return (false, "Brevo webhook secret is required");

        if (RequestTimeoutSeconds <= 0)
            return (false, "Request timeout must be greater than 0");

        if (MaxRetries < 0)
            return (false, "Max retries cannot be negative");

        if (InitialRetryDelayMilliseconds <= 0)
            return (false, "Initial retry delay must be greater than 0");

        if (RetryBackoffMultiplier <= 1.0)
            return (false, "Retry backoff multiplier must be greater than 1.0");

        if (!TemplateIds.Any(x => x.Value > 0))
            return (false, "At least one template ID must be configured");

        return (true, null);
    }

    /// <summary>
    /// Get template ID by template type name.
    /// </summary>
    public long GetTemplateId(string templateType)
    {
        if (TemplateIds.TryGetValue(templateType, out var templateId))
            return templateId;

        throw new KeyNotFoundException($"Template ID not configured for template type: {templateType}");
    }
}
