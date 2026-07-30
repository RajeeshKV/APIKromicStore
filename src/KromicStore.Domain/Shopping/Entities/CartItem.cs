using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shopping.Entities;

/// <summary>
/// CartItem entity representing a single item in a cart.
/// Part of the Cart aggregate root.
/// </summary>
public sealed class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public DateTime AddedOnUtc { get; private set; }

    private CartItem()
    {
    }

    private CartItem(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Create a new cart item.
    /// </summary>
    public static CartItem Create(Guid productId, Guid? variantId, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        var item = new CartItem(Guid.NewGuid())
        {
            ProductId = productId,
            ProductVariantId = variantId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            AddedOnUtc = DateTime.UtcNow
        };

        return item;
    }

    /// <summary>
    /// Update quantity.
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));

        Quantity = newQuantity;
    }

    /// <summary>
    /// Update unit price (for price changes).
    /// </summary>
    public void UpdateUnitPrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(newPrice));

        UnitPrice = newPrice;
    }

    /// <summary>
    /// Get the line total (Quantity × UnitPrice).
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;
}
