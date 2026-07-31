namespace KromicStore.Domain.Common;

/// <summary>
/// Webhook event for tracking and deduplication.
/// Ensures exactly-once webhook processing across all providers.
/// </summary>
public class WebhookEvent : BaseEntity
{
    public Guid? TenantId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string ExternalEventId { get; private set; } = string.Empty;
    public string Signature { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string? CorrelationId { get; private set; }
    public WebhookEventStatus Status { get; private set; } = WebhookEventStatus.Received;
    public int ProcessingAttempts { get; private set; }
    public DateTime ReceivedOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? FailedOnUtc { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? LastAttemptOnUtc { get; private set; }

    private WebhookEvent() { }

    private WebhookEvent(Guid id) : base(id) { }

    /// <summary>
    /// Creates a new webhook event record.
    /// </summary>
    public static WebhookEvent Create(
        string provider,
        string eventType,
        string externalEventId,
        string signature,
        string payload,
        Guid? tenantId = null,
        string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required.", nameof(provider));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type is required.", nameof(eventType));

        if (string.IsNullOrWhiteSpace(externalEventId))
            throw new ArgumentException("External event ID is required.", nameof(externalEventId));

        if (string.IsNullOrWhiteSpace(signature))
            throw new ArgumentException("Signature is required.", nameof(signature));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));

        return new WebhookEvent(Guid.NewGuid())
        {
            Provider = provider,
            EventType = eventType,
            ExternalEventId = externalEventId,
            Signature = signature,
            Payload = payload,
            TenantId = tenantId,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            Status = WebhookEventStatus.Received,
            ReceivedOnUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Mark webhook as being processed.
    /// </summary>
    public void MarkProcessing()
    {
        Status = WebhookEventStatus.Processing;
        ProcessingAttempts++;
        LastAttemptOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark webhook as successfully processed.
    /// </summary>
    public void MarkProcessed()
    {
        Status = WebhookEventStatus.Processed;
        ProcessedOnUtc = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Mark webhook as failed.
    /// </summary>
    public void MarkFailed(string errorMessage)
    {
        Status = WebhookEventStatus.Failed;
        FailedOnUtc = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        LastAttemptOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if webhook should be retried.
    /// </summary>
    public bool ShouldRetry(int maxRetries = 5)
    {
        return (Status == WebhookEventStatus.Failed || Status == WebhookEventStatus.Processing) &&
               ProcessingAttempts < maxRetries;
    }

    /// <summary>
    /// Mark webhook as dead-lettered.
    /// </summary>
    public void MarkDeadLettered(string reason)
    {
        Status = WebhookEventStatus.DeadLettered;
        FailedOnUtc = DateTime.UtcNow;
        ErrorMessage = reason;
    }

    /// <summary>
    /// Reset for retry.
    /// </summary>
    public void ResetForRetry()
    {
        Status = WebhookEventStatus.Received;
        FailedOnUtc = null;
        ErrorMessage = null;
        LastAttemptOnUtc = DateTime.UtcNow;
    }
}

/// <summary>
/// Webhook event processing status.
/// </summary>
public enum WebhookEventStatus
{
    /// <summary>
    /// Webhook received but not yet processed.
    /// </summary>
    Received = 0,

    /// <summary>
    /// Webhook is currently being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Webhook successfully processed.
    /// </summary>
    Processed = 2,

    /// <summary>
    /// Webhook processing failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Webhook exceeded max retries and is dead-lettered.
    /// </summary>
    DeadLettered = 4
}
