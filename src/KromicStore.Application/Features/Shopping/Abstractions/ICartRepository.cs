using KromicStore.Domain.Shopping.Entities;

namespace KromicStore.Application.Features.Shopping.Abstractions;

/// <summary>
/// Repository abstraction for Cart aggregate root.
/// Enforces tenant isolation and business rule validation.
/// </summary>
public interface ICartRepository
{
    /// <summary>
    /// Get cart by ID.
    /// </summary>
    Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active cart for a customer.
    /// </summary>
    Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cart by anonymous session ID.
    /// </summary>
    Task<Cart?> GetByAnonymousSessionIdAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if customer has an active cart.
    /// </summary>
    Task<bool> HasActiveCartAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new cart to the repository.
    /// </summary>
    void Add(Cart cart);

    /// <summary>
    /// Update an existing cart.
    /// </summary>
    void Update(Cart cart);

    /// <summary>
    /// Remove/delete a cart.
    /// </summary>
    void Remove(Cart cart);

    /// <summary>
    /// Get all expired carts (for cleanup).
    /// </summary>
    Task<IEnumerable<Cart>> GetExpiredCartsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to the repository.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
