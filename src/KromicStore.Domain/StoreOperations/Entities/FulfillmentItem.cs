using KromicStore.Domain.Common;

namespace KromicStore.Domain.StoreOperations.Entities;

/// <summary>
/// FulfillmentItem represents a single line item in a fulfillment.
/// </summary>
public sealed class FulfillmentItem : TenantEntity, IAuditable
{
    public Guid FulfillmentId { get; private set; }
    public Guid OrderLineItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public int PickedQuantity { get; private set; }
    public int PackedQuantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    
    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime ModifiedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ModifiedBy { get; private set; }
    
    private FulfillmentItem()
    {
    }
    
    private FulfillmentItem(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new fulfillment item.
    /// </summary>
    public static FulfillmentItem Create(
        Guid tenantId,
        Guid fulfillmentId,
        Guid orderLineItemId,
        Guid productId,
        string productName,
        string sku,
        int quantity,
        decimal unitPrice,
        string createdBy)
    {
        if (fulfillmentId == Guid.Empty)
            throw new ArgumentException("Fulfillment ID is required", nameof(fulfillmentId));
        
        if (orderLineItemId == Guid.Empty)
            throw new ArgumentException("Order line item ID is required", nameof(orderLineItemId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required", nameof(productName));
        
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required", nameof(sku));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by is required", nameof(createdBy));
        
        var item = new FulfillmentItem(Guid.NewGuid(), tenantId)
        {
            FulfillmentId = fulfillmentId,
            OrderLineItemId = orderLineItemId,
            ProductId = productId,
            ProductName = productName.Trim(),
            SKU = sku.Trim().ToUpperInvariant(),
            Quantity = quantity,
            PickedQuantity = 0,
            PackedQuantity = 0,
            UnitPrice = unitPrice,
            CreatedBy = createdBy.Trim()
        };
        
        return item;
    }
    
    /// <summary>
    /// Record quantity picked.
    /// </summary>
    public void RecordPickedQuantity(int pickedQuantity)
    {
        if (pickedQuantity < 0)
            throw new ArgumentException("Picked quantity cannot be negative", nameof(pickedQuantity));
        
        if (pickedQuantity > Quantity)
            throw new ArgumentException($"Picked quantity cannot exceed total quantity ({Quantity})", nameof(pickedQuantity));
        
        PickedQuantity = pickedQuantity;
    }
    
    /// <summary>
    /// Record quantity packed.
    /// </summary>
    public void RecordPackedQuantity(int packedQuantity)
    {
        if (packedQuantity < 0)
            throw new ArgumentException("Packed quantity cannot be negative", nameof(packedQuantity));
        
        if (packedQuantity > PickedQuantity)
            throw new ArgumentException($"Packed quantity cannot exceed picked quantity ({PickedQuantity})", nameof(packedQuantity));
        
        PackedQuantity = packedQuantity;
    }
    
    /// <summary>
    /// Check if all items have been picked.
    /// </summary>
    public bool IsFullyPicked() => PickedQuantity == Quantity;
    
    /// <summary>
    /// Check if all items have been packed.
    /// </summary>
    public bool IsFullyPacked() => PackedQuantity == PickedQuantity;
    
    /// <summary>
    /// Get the total price for this line item.
    /// </summary>
    public decimal GetTotalPrice() => Quantity * UnitPrice;
}
