using KromicStore.Domain.Common;

namespace KromicStore.Domain.Orders.Entities;

/// <summary>
/// OrderItem value object representing a single item in an order.
/// Snapshot of product information at time of order placement.
/// </summary>
public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? ProductVariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string ProductSku { get; private set; } = string.Empty;
    public string? VariantName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }
    
    // Item state
    public bool IsCancelled { get; private set; }
    public bool IsReturned { get; private set; }
    public int ReturnedQuantity { get; private set; }
    public DateTime? CancelledOnUtc { get; private set; }
    public DateTime? ReturnedOnUtc { get; private set; }
    
    private OrderItem()
    {
    }
    
    private OrderItem(Guid id) : base(id)
    {
    }
    
    /// <summary>
    /// Create a new order item.
    /// </summary>
    public static OrderItem Create(
        Guid orderId,
        Guid productId,
        string productName,
        string productSku,
        int quantity,
        decimal unitPrice,
        Guid? variantId = null,
        string? variantName = null)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be empty", nameof(productName));
        
        if (string.IsNullOrWhiteSpace(productSku))
            throw new ArgumentException("ProductSku cannot be empty", nameof(productSku));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
        
        if (unitPrice < 0)
            throw new ArgumentException("UnitPrice cannot be negative", nameof(unitPrice));
        
        var item = new OrderItem(Guid.NewGuid())
        {
            OrderId = orderId,
            ProductId = productId,
            ProductVariantId = variantId,
            ProductName = productName.Trim(),
            ProductSku = productSku.Trim(),
            VariantName = variantName?.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice
        };
        
        return item;
    }
    
    /// <summary>
    /// Cancel this item from the order.
    /// </summary>
    public void CancelItem()
    {
        if (IsCancelled)
            throw new InvalidOperationException("Item is already cancelled");
        
        IsCancelled = true;
        CancelledOnUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Mark item for return (full return).
    /// </summary>
    public void MarkForReturn()
    {
        if (IsCancelled)
            throw new InvalidOperationException("Cannot return a cancelled item");
        
        if (IsReturned)
            throw new InvalidOperationException("Item is already returned");
        
        IsReturned = true;
        ReturnedQuantity = Quantity;
        ReturnedOnUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Mark partial return of item.
    /// </summary>
    public void MarkPartialReturn(int returnedQuantity)
    {
        if (IsCancelled)
            throw new InvalidOperationException("Cannot return a cancelled item");
        
        if (returnedQuantity <= 0 || returnedQuantity > Quantity)
            throw new ArgumentException($"Returned quantity must be between 1 and {Quantity}", nameof(returnedQuantity));
        
        ReturnedQuantity = returnedQuantity;
        IsReturned = ReturnedQuantity == Quantity;
        ReturnedOnUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Get refund amount for this item.
    /// </summary>
    public decimal GetRefundAmount()
    {
        if (!IsCancelled && !IsReturned)
            return 0;
        
        var quantity = IsCancelled ? Quantity : ReturnedQuantity;
        return quantity * UnitPrice;
    }
}
