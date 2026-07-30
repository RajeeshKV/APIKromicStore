using KromicStore.Domain.Shopping.Entities;

namespace KromicStore.Application.Features.Shopping.Abstractions;

/// <summary>
/// Repository abstraction for Wishlist aggregate root.
/// Enforces tenant isolation and uniqueness constraints.
/// </summary>
public interface IWishlistRepository
{
    /// <summary>
    /// Get wishlist by ID.
    /// </summary>
    Task<Wishlist?> GetByIdAsync(Guid wishlistId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get wishlist for a specific customer.
    /// </summary>
    Task<Wishlist?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if customer has a wishlist.
    /// </summary>
    Task<bool> ExistsForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new wishlist to the repository.
    /// </summary>
    void Add(Wishlist wishlist);

    /// <summary>
    /// Update an existing wishlist.
    /// </summary>
    void Update(Wishlist wishlist);

    /// <summary>
    /// Remove/delete a wishlist.
    /// </summary>
    void Remove(Wishlist wishlist);

    /// <summary>
    /// Save changes to the repository.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
