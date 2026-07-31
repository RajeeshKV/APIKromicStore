using KromicStore.Domain.Common;

namespace KromicStore.Domain.Orders.Entities;

/// <summary>
/// Order aggregate root representing a placed order.
/// Contains order items, pricing, addresses, shipping, and payment information.
/// Manages the complete order lifecycle from placement to completion.
/// </summary>
public sealed class Order : TenantEntity, IAuditable, ISoftDeletable
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid BillingAddressId { get; private set; }
    public Guid ShippingAddressId { get; private set; }
    public string ShippingMethod { get; private set; } = string.Empty;
    public string PaymentMethod { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    
    // Pricing
    public decimal SubTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal ShippingAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal GrandTotal { get; private set; }
    
    // Applied coupon/promotion
    public string? CouponCode { get; private set; }
    
    // Notes
    public string? Notes { get; private set; }
    
    // Timestamps (CreatedOnUtc is inherited from AuditableEntity)
    public DateTime? ShippedOnUtc { get; private set; }
    public DateTime? DeliveredOnUtc { get; private set; }
    public DateTime? CancelledOnUtc { get; private set; }
    
    // Relationships
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    
    private readonly List<OrderTimeline> _timeline = [];
    public IReadOnlyList<OrderTimeline> Timeline => _timeline.AsReadOnly();
    
    private readonly List<OrderNote> _notes = [];
    public IReadOnlyList<OrderNote> OrderNotes => _notes.AsReadOnly();
    
    // Payment relationship
    public Guid? PaymentId { get; private set; }
    
    private Order()
    {
    }
    
    private Order(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new order from checkout data.
    /// </summary>
    public static Order Create(
        Guid tenantId,
        Guid customerId,
        string orderNumber,
        Guid billingAddressId,
        Guid shippingAddressId,
        string shippingMethod,
        string paymentMethod,
        List<OrderItem> items,
        decimal subTotal,
        decimal discountAmount,
        decimal shippingAmount,
        decimal taxAmount,
        string? couponCode = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));
        
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));
        
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("OrderNumber cannot be empty", nameof(orderNumber));
        
        if (items == null || items.Count == 0)
            throw new ArgumentException("Order must have at least one item", nameof(items));
        
        if (subTotal < 0)
            throw new ArgumentException("SubTotal cannot be negative", nameof(subTotal));
        
        if (discountAmount < 0)
            throw new ArgumentException("DiscountAmount cannot be negative", nameof(discountAmount));
        
        if (shippingAmount < 0)
            throw new ArgumentException("ShippingAmount cannot be negative", nameof(shippingAmount));
        
        if (taxAmount < 0)
            throw new ArgumentException("TaxAmount cannot be negative", nameof(taxAmount));
        
        var order = new Order(Guid.NewGuid(), tenantId)
        {
            CustomerId = customerId,
            OrderNumber = orderNumber.Trim(),
            BillingAddressId = billingAddressId,
            ShippingAddressId = shippingAddressId,
            ShippingMethod = shippingMethod.Trim(),
            PaymentMethod = paymentMethod.Trim(),
            Status = OrderStatus.Pending,
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            ShippingAmount = shippingAmount,
            TaxAmount = taxAmount,
            CouponCode = couponCode?.ToUpperInvariant()
        };
        
        // Mark as created (inherited from AuditableEntity)
        order.MarkCreated(DateTime.UtcNow, "System");
        
        // Add items
        foreach (var item in items)
        {
            order._items.Add(item);
        }
        
        order.RecalculateGrandTotal();
        
        // Add initial timeline entry
        order._timeline.Add(OrderTimeline.Create(order.Id, "Order created", "System"));
        
        return order;
    }
    
    /// <summary>
    /// Confirm order (payment accepted, preparing for shipment).
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Can only confirm pending orders. Current status: {Status}");
        
        Status = OrderStatus.Confirmed;
        _timeline.Add(OrderTimeline.Create(Id, "Order confirmed", "System"));
    }
    
    /// <summary>
    /// Mark order as shipped.
    /// </summary>
    public void MarkAsShipped(string trackingNumber = "")
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Can only ship confirmed orders. Current status: {Status}");
        
        Status = OrderStatus.Shipped;
        ShippedOnUtc = DateTime.UtcNow;
        _timeline.Add(OrderTimeline.Create(Id, $"Order shipped. Tracking: {trackingNumber}", "System"));
    }
    
    /// <summary>
    /// Mark order as delivered.
    /// </summary>
    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Can only deliver shipped orders. Current status: {Status}");
        
        Status = OrderStatus.Delivered;
        DeliveredOnUtc = DateTime.UtcNow;
        _timeline.Add(OrderTimeline.Create(Id, "Order delivered", "System"));
    }
    
    /// <summary>
    /// Cancel the entire order.
    /// </summary>
    public void Cancel(string reason = "")
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel {Status} orders");
        
        Status = OrderStatus.Cancelled;
        CancelledOnUtc = DateTime.UtcNow;
        _timeline.Add(OrderTimeline.Create(Id, $"Order cancelled. Reason: {reason}", "System"));
    }
    
    /// <summary>
    /// Request partial cancellation of specific items.
    /// </summary>
    public void RequestPartialCancellation(List<Guid> itemIds, string reason = "")
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel items from {Status} orders");
        
        // Mark items as cancelled (implementation in OrderItem)
        foreach (var itemId in itemIds)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.CancelItem();
            }
        }
        
        _timeline.Add(OrderTimeline.Create(Id, $"Partial cancellation requested. Reason: {reason}", "System"));
    }
    
    /// <summary>
    /// Request a return for specific items.
    /// </summary>
    public void RequestReturn(List<Guid> itemIds, string reason = "")
    {
        if (Status != OrderStatus.Delivered)
            throw new InvalidOperationException($"Can only request returns for delivered orders. Current status: {Status}");
        
        foreach (var itemId in itemIds)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.MarkForReturn();
            }
        }
        
        _timeline.Add(OrderTimeline.Create(Id, $"Return requested. Reason: {reason}", "System"));
    }
    
    /// <summary>
    /// Add note to order.
    /// </summary>
    public void AddNote(string content, string author = "System")
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Note content cannot be empty", nameof(content));
        
        _notes.Add(OrderNote.Create(Id, content, author));
    }
    
    /// <summary>
    /// Link payment to order.
    /// </summary>
    public void LinkPayment(Guid paymentId)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentException("PaymentId cannot be empty", nameof(paymentId));
        
        PaymentId = paymentId;
    }
    
    /// <summary>
    /// Get total items count (sum of quantities).
    /// </summary>
    public int GetTotalItemsCount() => _items.Sum(i => i.Quantity);
    
    /// <summary>
    /// Get total items (excluding cancelled).
    /// </summary>
    public int GetActiveItemsCount() => _items.Where(i => !i.IsCancelled).Sum(i => i.Quantity);
    
    /// <summary>
    /// Check if all items have been returned.
    /// </summary>
    public bool AllItemsReturned => _items.All(i => i.IsReturned);
    
    /// <summary>
    /// Check if order can be processed for refund.
    /// </summary>
    public bool CanProcessRefund => Status == OrderStatus.Cancelled || AllItemsReturned;
    
    /// <summary>
    /// Recalculate grand total.
    /// </summary>
    private void RecalculateGrandTotal()
    {
        GrandTotal = SubTotal - DiscountAmount + ShippingAmount + TaxAmount;
        
        if (GrandTotal < 0)
            GrandTotal = 0;
    }
}

/// <summary>
/// Order status enumeration.
/// </summary>
public enum OrderStatus
{
    Pending = 0,           // Awaiting confirmation/payment
    Confirmed = 1,         // Payment confirmed, awaiting shipment
    Shipped = 2,           // Shipped to customer
    Delivered = 3,         // Delivered to customer
    Cancelled = 4,         // Order cancelled
    PartiallyReturned = 5, // Some items returned
    Returned = 6           // All items returned
}
