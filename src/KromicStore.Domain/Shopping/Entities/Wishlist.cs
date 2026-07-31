using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shopping.Entities;

/// <summary>
/// Wishlist aggregate root representing a customer's wishlist.
/// One wishlist per customer. Supports unlimited wish items.
/// </summary>
public sealed class Wishlist : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid CustomerId { get; private set; }

    // Relationships
    private readonly List<WishlistItem> _items = [];
    public IReadOnlyList<WishlistItem> Items => _items.AsReadOnly();

    // Auditing and soft delete are inherited from AuditableEntity

    private Wishlist()
    {
    }

    private Wishlist(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }

    /// <summary>
    /// Create a new wishlist for a customer.
    /// </summary>
    public static Wishlist Create(Guid tenantId, Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));

        var wishlist = new Wishlist(Guid.NewGuid(), tenantId)
        {
            CustomerId = customerId
        };

        return wishlist;
    }

    /// <summary>
    /// Add a product to the wishlist.
    /// Duplicate items are not allowed.
    /// </summary>
    public void AddItem(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        if (IsDeleted)
            throw new InvalidOperationException("Cannot add items to a deleted wishlist");

        // Check for duplicate
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
            throw new InvalidOperationException($"Product {productId} is already in the wishlist");

        var item = WishlistItem.Create(productId);
        _items.Add(item);
    }

    /// <summary>
    /// Remove a product from the wishlist.
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        if (IsDeleted)
            throw new InvalidOperationException("Cannot remove items from a deleted wishlist");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    /// <summary>
    /// Check if a product is in the wishlist.
    /// </summary>
    public bool ContainsProduct(Guid productId) =>
        _items.Any(i => i.ProductId == productId);

    /// <summary>
    /// Get the number of items in the wishlist.
    /// </summary>
    public int GetItemsCount() => _items.Count;

    /// <summary>
    /// Clear all items from the wishlist.
    /// </summary>
    public void Clear()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot clear a deleted wishlist");

        _items.Clear();
    }

    /// <summary>
    /// Check if wishlist is empty.
    /// </summary>
    public bool IsEmpty => _items.Count == 0;
}
