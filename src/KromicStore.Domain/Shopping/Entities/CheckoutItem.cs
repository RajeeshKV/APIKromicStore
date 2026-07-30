using KromicStore.Domain.Common;

namespace KromicStore.Domain.Shopping.Entities;

/// <summary>
/// CheckoutItem entity representing a product in a checkout session.
/// Part of the CheckoutSession aggregate root.
/// Stores snapshot of product information for historical accuracy.
/// </summary>
public sealed class CheckoutItem : BaseEntity
{
    public Guid CheckoutSessionId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private CheckoutItem()
    {
    }

    private CheckoutItem(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Create a new checkout item.
    /// </summary>
    public static CheckoutItem Create(Guid productId, Guid? variantId, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        var item = new CheckoutItem(Guid.NewGuid())
        {
            ProductId = productId,
            ProductVariantId = variantId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };

        return item;
    }

    /// <summary>
    /// Get the line total (Quantity × UnitPrice).
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;
}
