using System.Text.Json;

namespace KromicStore.Domain.Common;

/// <summary>
/// Outbox event for exactly-once message processing.
/// Implements the Outbox pattern to guarantee reliable event publishing.
/// </summary>
public class OutboxEvent : BaseEntity
{
    public Guid? TenantId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? CorrelationId { get; private set; }
    public string? CausationId { get; private set; }
    public OutboxEventStatus Status { get; private set; } = OutboxEventStatus.Pending;
    public int RetryCount { get; private set; }
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? FailedOnUtc { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? LastRetryOnUtc { get; private set; }

    private OutboxEvent() { }

    private OutboxEvent(Guid id) : base(id) { }

    /// <summary>
    /// Creates a new outbox event from a domain event.
    /// </summary>
    public static OutboxEvent Create<TEvent>(
        TEvent domainEvent,
        Guid? tenantId = null,
        string? correlationId = null,
        string? causationId = null) where TEvent : IDomainEvent
    {
        var eventType = domainEvent.GetType().FullName ??
                       throw new InvalidOperationException("Event type name is required.");

        var content = JsonSerializer.Serialize(domainEvent);

        var outboxEvent = new OutboxEvent(Guid.NewGuid())
        {
            TenantId = tenantId,
            EventType = eventType,
            Content = content,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            CausationId = causationId,
            Status = OutboxEventStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow
        };

        return outboxEvent;
    }

    /// <summary>
    /// Mark event as processed successfully.
    /// </summary>
    public void MarkProcessed()
    {
        Status = OutboxEventStatus.Processed;
        ProcessedOnUtc = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Mark event as failed with error message.
    /// </summary>
    public void MarkFailed(string errorMessage)
    {
        Status = OutboxEventStatus.Failed;
        FailedOnUtc = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        LastRetryOnUtc = DateTime.UtcNow;
        RetryCount++;
    }

    /// <summary>
    /// Reset event for retry.
    /// </summary>
    public void ResetForRetry()
    {
        Status = OutboxEventStatus.Pending;
        FailedOnUtc = null;
        ErrorMessage = null;
        LastRetryOnUtc = DateTime.UtcNow;
        RetryCount++;
    }

    /// <summary>
    /// Check if event should be retried.
    /// </summary>
    public bool ShouldRetry(int maxRetries = 5)
    {
        return Status == OutboxEventStatus.Failed && RetryCount < maxRetries;
    }

    /// <summary>
    /// Mark event as dead-lettered (too many retries).
    /// </summary>
    public void MarkDeadLettered(string reason)
    {
        Status = OutboxEventStatus.DeadLettered;
        FailedOnUtc = DateTime.UtcNow;
        ErrorMessage = reason;
    }

    /// <summary>
    /// Deserialize event content back to domain event.
    /// </summary>
    public T? GetEvent<T>() where T : IDomainEvent
    {
        try
        {
            return JsonSerializer.Deserialize<T>(Content);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}

/// <summary>
/// Status of outbox event processing.
/// </summary>
public enum OutboxEventStatus
{
    /// <summary>
    /// Event is pending processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Event has been successfully processed.
    /// </summary>
    Processed = 1,

    /// <summary>
    /// Event processing failed and is being retried.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Event has exceeded maximum retry count and is dead-lettered.
    /// </summary>
    DeadLettered = 3
}
