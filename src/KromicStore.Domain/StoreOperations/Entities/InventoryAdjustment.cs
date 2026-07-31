using KromicStore.Domain.Common;

namespace KromicStore.Domain.StoreOperations.Entities;

/// <summary>
/// Adjustment reason types for inventory changes.
/// </summary>
public enum AdjustmentReason
{
    Damage = 0,
    Loss = 1,
    Miscount = 2,
    Restock = 3,
    Return = 4,
    Correction = 5,
    Expiration = 6
}

/// <summary>
/// Adjustment status workflow states.
/// </summary>
public enum AdjustmentStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Applied = 3
}

/// <summary>
/// InventoryAdjustment represents a change to inventory quantity.
/// Follows an approval workflow: Pending → Approved → Applied or Rejected.
/// </summary>
public sealed class InventoryAdjustment : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid ProductId { get; private set; }
    public int AdjustmentQuantity { get; private set; } // Positive or negative
    public AdjustmentReason Reason { get; private set; }
    public string ReasonNotes { get; private set; } = string.Empty;
    public AdjustmentStatus Status { get; private set; }
    
    // Approval workflow
    public DateTime RequestedOnUtc { get; private set; }
    public string RequestedBy { get; private set; } = string.Empty;
    public DateTime? ApprovedOnUtc { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? RejectedOnUtc { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? AppliedOnUtc { get; private set; }
    
    // Auditing and soft delete are inherited from AuditableEntity
    
    private InventoryAdjustment()
    {
    }
    
    private InventoryAdjustment(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new inventory adjustment request.
    /// </summary>
    public static InventoryAdjustment Create(
        Guid tenantId,
        Guid productId,
        int quantity,
        AdjustmentReason reason,
        string reasonNotes,
        string requestedBy)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required", nameof(productId));
        
        if (quantity == 0)
            throw new ArgumentException("Adjustment quantity cannot be zero", nameof(quantity));
        
        if (string.IsNullOrWhiteSpace(reasonNotes))
            throw new ArgumentException("Reason notes are required", nameof(reasonNotes));
        
        if (string.IsNullOrWhiteSpace(requestedBy))
            throw new ArgumentException("Requested by is required", nameof(requestedBy));
        
        var adjustment = new InventoryAdjustment(Guid.NewGuid(), tenantId)
        {
            ProductId = productId,
            AdjustmentQuantity = quantity,
            Reason = reason,
            ReasonNotes = reasonNotes.Trim(),
            Status = AdjustmentStatus.Pending,
            RequestedOnUtc = DateTime.UtcNow,
            RequestedBy = requestedBy.Trim()
        };
        
        return adjustment;
    }
    
    /// <summary>
    /// Approve the adjustment request.
    /// </summary>
    public void Approve(string approvedBy)
    {
        if (Status != AdjustmentStatus.Pending)
            throw new InvalidOperationException($"Cannot approve adjustment with status {Status}");
        
        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("Approved by is required", nameof(approvedBy));
        
        Status = AdjustmentStatus.Approved;
        ApprovedOnUtc = DateTime.UtcNow;
        ApprovedBy = approvedBy.Trim();
    }
    
    /// <summary>
    /// Reject the adjustment request.
    /// </summary>
    public void Reject(string rejectionReason, string rejectedBy)
    {
        if (Status != AdjustmentStatus.Pending)
            throw new InvalidOperationException($"Cannot reject adjustment with status {Status}");
        
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Rejection reason is required", nameof(rejectionReason));
        
        if (string.IsNullOrWhiteSpace(rejectedBy))
            throw new ArgumentException("Rejected by is required", nameof(rejectedBy));
        
        Status = AdjustmentStatus.Rejected;
        RejectedOnUtc = DateTime.UtcNow;
        RejectionReason = rejectionReason.Trim();
        ApprovedBy = rejectedBy.Trim(); // Store who rejected it
    }
    
    /// <summary>
    /// Apply the approved adjustment to inventory.
    /// </summary>
    public void Apply()
    {
        if (Status != AdjustmentStatus.Approved)
            throw new InvalidOperationException($"Cannot apply adjustment with status {Status}");
        
        Status = AdjustmentStatus.Applied;
        AppliedOnUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Check if adjustment can be approved.
    /// </summary>
    public bool CanApprove() => Status == AdjustmentStatus.Pending;
    
    /// <summary>
    /// Check if adjustment can be rejected.
    /// </summary>
    public bool CanReject() => Status == AdjustmentStatus.Pending;
    
    /// <summary>
    /// Check if adjustment can be applied.
    /// </summary>
    public bool CanApply() => Status == AdjustmentStatus.Approved;
}
