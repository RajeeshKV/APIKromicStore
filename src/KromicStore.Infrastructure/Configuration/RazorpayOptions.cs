using System.ComponentModel.DataAnnotations;

namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// Razorpay payment gateway configuration.
/// Strongly typed configuration with validation.
/// </summary>
public class RazorpayOptions
{
    public const string SectionName = "Razorpay";

    [Required(ErrorMessage = "Razorpay Key ID is required")]
    public string KeyId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Razorpay Key Secret is required")]
    public string KeySecret { get; set; } = string.Empty;

    [Required(ErrorMessage = "Razorpay webhook secret is required")]
    public string WebhookSecret { get; set; } = string.Empty;

    [Url(ErrorMessage = "Razorpay base URL must be a valid URL")]
    public string BaseUrl { get; set; } = "https://api.razorpay.com/v1";

    public int RequestTimeoutSeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 3;

    public int InitialRetryDelayMilliseconds { get; set; } = 1000;

    public double RetryBackoffMultiplier { get; set; } = 2.0;

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Default currency for payments.
    /// </summary>
    public string DefaultCurrency { get; set; } = "INR";

    /// <summary>
    /// Allowed currencies for payments.
    /// </summary>
    public List<string> AllowedCurrencies { get; set; } = new()
    {
        "INR",
        "USD",
        "EUR",
        "GBP"
    };

    /// <summary>
    /// Enable webhook signature verification.
    /// </summary>
    public bool EnableWebhookVerification { get; set; } = true;

    /// <summary>
    /// Webhook verification retry count.
    /// </summary>
    public int WebhookRetryCount { get; set; } = 5;

    /// <summary>
    /// Validate configuration.
    /// </summary>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (string.IsNullOrWhiteSpace(KeyId))
            return (false, "Razorpay Key ID is required");

        if (KeyId.Length < 10)
            return (false, "Razorpay Key ID appears to be invalid (too short)");

        if (string.IsNullOrWhiteSpace(KeySecret))
            return (false, "Razorpay Key Secret is required");

        if (KeySecret.Length < 20)
            return (false, "Razorpay Key Secret appears to be invalid (too short)");

        if (string.IsNullOrWhiteSpace(WebhookSecret))
            return (false, "Razorpay webhook secret is required");

        if (string.IsNullOrWhiteSpace(DefaultCurrency) || DefaultCurrency.Length != 3)
            return (false, "Default currency must be a 3-letter ISO code");

        if (!AllowedCurrencies.Any())
            return (false, "At least one currency must be allowed");

        if (!AllowedCurrencies.Contains(DefaultCurrency))
            return (false, "Default currency must be in allowed currencies list");

        if (RequestTimeoutSeconds <= 0)
            return (false, "Request timeout must be greater than 0");

        if (MaxRetries < 0)
            return (false, "Max retries cannot be negative");

        if (InitialRetryDelayMilliseconds <= 0)
            return (false, "Initial retry delay must be greater than 0");

        if (RetryBackoffMultiplier <= 1.0)
            return (false, "Retry backoff multiplier must be greater than 1.0");

        if (WebhookRetryCount < 0)
            return (false, "Webhook retry count cannot be negative");

        return (true, null);
    }

    /// <summary>
    /// Check if currency is allowed.
    /// </summary>
    public bool IsCurrencyAllowed(string currency)
    {
        return AllowedCurrencies.Contains(currency.ToUpperInvariant());
    }
}
