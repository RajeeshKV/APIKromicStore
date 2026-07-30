using KromicStore.Domain.Shopping.Entities;

namespace KromicStore.Application.Features.Shopping.Abstractions;

/// <summary>
/// Repository abstraction for CheckoutSession aggregate root.
/// Enforces tenant isolation and checkout state management.
/// </summary>
public interface ICheckoutSessionRepository
{
    /// <summary>
    /// Get checkout session by ID.
    /// </summary>
    Task<CheckoutSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active checkout session for a customer.
    /// Active means not completed, not expired, not cancelled.
    /// </summary>
    Task<CheckoutSession?> GetActiveByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all checkout sessions for a customer (including historical).
    /// </summary>
    Task<IEnumerable<CheckoutSession>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if customer has an active checkout.
    /// </summary>
    Task<bool> HasActiveCheckoutAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new checkout session.
    /// </summary>
    void Add(CheckoutSession session);

    /// <summary>
    /// Update an existing checkout session.
    /// </summary>
    void Update(CheckoutSession session);

    /// <summary>
    /// Remove/delete a checkout session.
    /// </summary>
    void Remove(CheckoutSession session);

    /// <summary>
    /// Get all expired checkout sessions (for cleanup).
    /// </summary>
    Task<IEnumerable<CheckoutSession>> GetExpiredSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to the repository.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
