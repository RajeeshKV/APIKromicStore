using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shopping.Entities;

/// <summary>
/// Cart aggregate root representing a shopping cart.
/// One active cart per customer or guest session.
/// Supports both authenticated customers and anonymous guests.
/// </summary>
public sealed class Cart : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid? CustomerId { get; private set; }
    public string? AnonymousSessionId { get; private set; }
    public string Currency { get; private set; } = "USD";
    public DateTime LastActivityOnUtc { get; private set; }
    public DateTime ExpiresOnUtc { get; private set; }

    // Relationships
    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    // Auditing
    public DateTime CreatedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime ModifiedAtUtc { get; private set; }
    public string ModifiedBy { get; private set; } = string.Empty;

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private Cart()
    {
    }

    private Cart(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }

    /// <summary>
    /// Create a new cart for a customer.
    /// </summary>
    public static Cart CreateForCustomer(Guid tenantId, Guid customerId, string currency = "USD")
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty for customer cart", nameof(customerId));

        var cart = new Cart(Guid.NewGuid(), tenantId)
        {
            CustomerId = customerId,
            AnonymousSessionId = null,
            Currency = ValidateCurrency(currency),
            LastActivityOnUtc = DateTime.UtcNow,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(30) // 30-day expiration for inactive carts
        };

        return cart;
    }

    /// <summary>
    /// Create a new cart for an anonymous guest.
    /// </summary>
    public static Cart CreateForGuest(Guid tenantId, string sessionId, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId cannot be empty for guest cart", nameof(sessionId));

        var cart = new Cart(Guid.NewGuid(), tenantId)
        {
            CustomerId = null,
            AnonymousSessionId = sessionId.Trim(),
            Currency = ValidateCurrency(currency),
            LastActivityOnUtc = DateTime.UtcNow,
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7) // 7-day expiration for guest carts
        };

        return cart;
    }

    /// <summary>
    /// Add an item to the cart or increase quantity if already present.
    /// </summary>
    public void AddItem(Guid productId, decimal unitPrice, int quantity = 1, Guid? variantId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (IsDeleted)
            throw new InvalidOperationException("Cannot add items to a deleted cart");

        // Check if item already exists
        var existingItem = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.ProductVariantId == variantId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = CartItem.Create(productId, variantId, quantity, unitPrice);
            _items.Add(newItem);
        }

        UpdateActivity();
    }

    /// <summary>
    /// Update quantity for a cart item.
    /// </summary>
    public void UpdateItemQuantity(Guid productId, int newQuantity, Guid? variantId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        if (newQuantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));

        if (IsDeleted)
            throw new InvalidOperationException("Cannot update items in a deleted cart");

        var item = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.ProductVariantId == variantId);

        if (item == null)
            throw new InvalidOperationException($"Cart item not found: ProductId={productId}, VariantId={variantId}");

        if (newQuantity == 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(newQuantity);
        }

        UpdateActivity();
    }

    /// <summary>
    /// Remove a specific item from cart.
    /// </summary>
    public void RemoveItem(Guid productId, Guid? variantId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        if (IsDeleted)
            throw new InvalidOperationException("Cannot remove items from a deleted cart");

        var item = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.ProductVariantId == variantId);

        if (item != null)
        {
            _items.Remove(item);
            UpdateActivity();
        }
    }

    /// <summary>
    /// Clear all items from the cart.
    /// </summary>
    public void Clear()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot clear a deleted cart");

        _items.Clear();
        UpdateActivity();
    }

    /// <summary>
    /// Check if cart is empty.
    /// </summary>
    public bool IsEmpty => _items.Count == 0;

    /// <summary>
    /// Get total items count (sum of quantities).
    /// </summary>
    public int GetItemsCount() => _items.Sum(i => i.Quantity);

    /// <summary>
    /// Get cart subtotal.
    /// </summary>
    public decimal GetSubtotal() => _items.Sum(i => i.LineTotal);

    /// <summary>
    /// Check if cart has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresOnUtc;

    /// <summary>
    /// Check if cart contains a specific item.
    /// </summary>
    public bool HasItem(Guid productId, Guid? variantId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        return _items.Any(i => i.ProductId == productId && i.ProductVariantId == variantId);
    }

    /// <summary>
    /// Soft delete the cart.
    /// </summary>
    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Convert guest cart to customer cart.
    /// </summary>
    public void ConvertToCustomerCart(Guid customerId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));

        if (CustomerId.HasValue)
            throw new InvalidOperationException("Cart is already associated with a customer");

        CustomerId = customerId;
        AnonymousSessionId = null;
        ExpiresOnUtc = DateTime.UtcNow.AddDays(30); // Extend expiration for customer cart
        UpdateActivity();
    }

    /// <summary>
    /// Update last activity timestamp.
    /// </summary>
    private void UpdateActivity()
    {
        LastActivityOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Validate currency code (ISO 4217).
    /// </summary>
    private static string ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a valid ISO 4217 code (3 characters)", nameof(currency));

        return currency.ToUpperInvariant();
    }
}
