using KromicStore.Domain.Common;

namespace KromicStore.Domain.StoreOperations.Entities;

/// <summary>
/// Fulfillment status workflow states.
/// </summary>
public enum FulfillmentStatus
{
    Pending = 0,
    Processing = 1,
    Packed = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

/// <summary>
/// Fulfillment represents the process of preparing and shipping an order.
/// Manages the workflow from receipt to delivery.
/// </summary>
public sealed class Fulfillment : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid OrderId { get; private set; }
    public FulfillmentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public DateTime? PackedAtUtc { get; private set; }
    public DateTime? ShippedAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    
    // Shipping information
    public string? TrackingNumber { get; private set; }
    public string? CarrierCode { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public decimal ShippingCost { get; private set; }
    
    // Notes
    public string? ProcessingNotes { get; private set; }
    public string? PackingNotes { get; private set; }
    public string? ShippingNotes { get; private set; }
    
    // Items collection
    private readonly List<FulfillmentItem> _items = new();
    public IReadOnlyList<FulfillmentItem> Items => _items.AsReadOnly();
    
    // Auditing
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime ModifiedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? ModifiedBy { get; private set; }
    
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }
    
    private Fulfillment()
    {
    }
    
    private Fulfillment(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new fulfillment for an order.
    /// </summary>
    public static Fulfillment Create(
        Guid tenantId,
        Guid orderId,
        string shippingAddress,
        decimal shippingCost,
        string createdBy)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID is required", nameof(orderId));
        
        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address is required", nameof(shippingAddress));
        
        if (shippingCost < 0)
            throw new ArgumentException("Shipping cost cannot be negative", nameof(shippingCost));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by is required", nameof(createdBy));
        
        var fulfillment = new Fulfillment(Guid.NewGuid(), tenantId)
        {
            OrderId = orderId,
            Status = FulfillmentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            ShippingAddress = shippingAddress.Trim(),
            ShippingCost = shippingCost,
            CreatedBy = createdBy.Trim()
        };
        
        return fulfillment;
    }
    
    /// <summary>
    /// Add an item to the fulfillment.
    /// </summary>
    public void AddItem(FulfillmentItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        
        if (Status != FulfillmentStatus.Pending)
            throw new InvalidOperationException($"Cannot add items to fulfillment with status {Status}");
        
        _items.Add(item);
    }
    
    /// <summary>
    /// Mark fulfillment as processing.
    /// </summary>
    public void MarkAsProcessing(string? notes = null)
    {
        if (Status != FulfillmentStatus.Pending)
            throw new InvalidOperationException($"Cannot process fulfillment with status {Status}");
        
        Status = FulfillmentStatus.Processing;
        ProcessedAtUtc = DateTime.UtcNow;
        ProcessingNotes = notes?.Trim();
    }
    
    /// <summary>
    /// Mark fulfillment as packed.
    /// </summary>
    public void MarkAsPacked(string? notes = null)
    {
        if (Status != FulfillmentStatus.Processing)
            throw new InvalidOperationException($"Cannot pack fulfillment with status {Status}");
        
        Status = FulfillmentStatus.Packed;
        PackedAtUtc = DateTime.UtcNow;
        PackingNotes = notes?.Trim();
    }
    
    /// <summary>
    /// Mark fulfillment as shipped with tracking.
    /// </summary>
    public void MarkAsShipped(string trackingNumber, string carrierCode, string? notes = null)
    {
        if (Status != FulfillmentStatus.Packed)
            throw new InvalidOperationException($"Cannot ship fulfillment with status {Status}");
        
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required", nameof(trackingNumber));
        
        if (string.IsNullOrWhiteSpace(carrierCode))
            throw new ArgumentException("Carrier code is required", nameof(carrierCode));
        
        Status = FulfillmentStatus.Shipped;
        ShippedAtUtc = DateTime.UtcNow;
        TrackingNumber = trackingNumber.Trim();
        CarrierCode = carrierCode.Trim();
        ShippingNotes = notes?.Trim();
    }
    
    /// <summary>
    /// Mark fulfillment as delivered.
    /// </summary>
    public void MarkAsDelivered()
    {
        if (Status != FulfillmentStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver fulfillment with status {Status}");
        
        Status = FulfillmentStatus.Delivered;
        DeliveredAtUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Cancel the fulfillment.
    /// </summary>
    public void Cancel()
    {
        if (Status == FulfillmentStatus.Delivered || Status == FulfillmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel fulfillment with status {Status}");
        
        Status = FulfillmentStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Update tracking information.
    /// </summary>
    public void UpdateTrackingNumber(string trackingNumber, string carrierCode)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required", nameof(trackingNumber));
        
        if (string.IsNullOrWhiteSpace(carrierCode))
            throw new ArgumentException("Carrier code is required", nameof(carrierCode));
        
        if (Status != FulfillmentStatus.Shipped)
            throw new InvalidOperationException("Can only update tracking for shipped fulfillments");
        
        TrackingNumber = trackingNumber.Trim();
        CarrierCode = carrierCode.Trim();
    }
    
    /// <summary>
    /// Get total item count.
    /// </summary>
    public int GetTotalItemCount() => _items.Sum(i => i.Quantity);
    
    /// <summary>
    /// Check if fulfillment can be processed.
    /// </summary>
    public bool CanProcess() => Status == FulfillmentStatus.Pending && _items.Count > 0;
    
    /// <summary>
    /// Check if fulfillment can be packed.
    /// </summary>
    public bool CanPack() => Status == FulfillmentStatus.Processing;
    
    /// <summary>
    /// Check if fulfillment can be shipped.
    /// </summary>
    public bool CanShip() => Status == FulfillmentStatus.Packed;
}
