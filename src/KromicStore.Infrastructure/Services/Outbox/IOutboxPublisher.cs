using KromicStore.Domain.Common;

namespace KromicStore.Infrastructure.Services.Outbox;

/// <summary>
/// Outbox publisher for exactly-once event publishing.
/// Implements the Outbox pattern to guarantee reliable event delivery.
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Publish a domain event to the outbox for later processing.
    /// Transaction-safe: event is stored with the same transaction as the command.
    /// </summary>
    Task PublishAsync<TEvent>(
        TEvent domainEvent,
        Guid? tenantId = null,
        string? correlationId = null,
        string? causationId = null,
        CancellationToken cancellationToken = default) where TEvent : IDomainEvent;

    /// <summary>
    /// Process pending outbox events.
    /// Should be called by background worker periodically.
    /// </summary>
    Task ProcessPendingEventsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry failed outbox events.
    /// Should be called by background worker periodically.
    /// </summary>
    Task RetryFailedEventsAsync(int maxRetries = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get failed events that need dead-letter processing.
    /// </summary>
    Task<List<OutboxEvent>> GetDeadLetteredEventsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Health check - verify outbox table connectivity.
    /// </summary>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}
