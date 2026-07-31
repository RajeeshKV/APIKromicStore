using KromicStore.Domain.Email.Entities;

namespace KromicStore.Application.Features.Email.Abstractions;

/// <summary>
/// Repository for email outbox operations.
/// Manages reliable email delivery with retry logic.
/// </summary>
public interface IEmailOutboxRepository
{
    /// <summary>
    /// Add email to outbox.
    /// </summary>
    Task AddAsync(EmailOutbox email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get email by ID.
    /// </summary>
    Task<EmailOutbox?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending emails for processing (not yet attempted).
    /// </summary>
    Task<IEnumerable<EmailOutbox>> GetPendingAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get emails ready for retry (NextRetryAtUtc has passed).
    /// </summary>
    Task<IEnumerable<EmailOutbox>> GetReadyForRetryAsync(int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get emails by status.
    /// </summary>
    Task<IEnumerable<EmailOutbox>> GetByStatusAsync(EmailOutboxStatus status, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update email status.
    /// </summary>
    Task UpdateAsync(EmailOutbox email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete old processed emails (older than specified days).
    /// </summary>
    Task<int> DeleteOldProcessedAsync(int olderThanDays = 30, CancellationToken cancellationToken = default);
}
