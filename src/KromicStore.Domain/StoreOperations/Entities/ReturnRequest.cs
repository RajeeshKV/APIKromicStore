using KromicStore.Domain.Common;

namespace KromicStore.Domain.StoreOperations.Entities;

/// <summary>
/// Return request status workflow states.
/// </summary>
public enum ReturnStatus
{
    Requested = 0,
    Approved = 1,
    Rejected = 2,
    Received = 3,
    InInspection = 4,
    Completed = 5,
    Cancelled = 6
}

/// <summary>
/// ReturnRequest represents a customer's request to return items from an order.
/// Manages the workflow from request to completion (inspection, approval, refund).
/// </summary>
public sealed class ReturnRequest : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public ReturnStatus Status { get; private set; }
    
    // Request details
    public DateTime RequestedOnUtc { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? CustomerNotes { get; private set; }
    public int ItemCount { get; private set; }
    public decimal ReturnAmount { get; private set; }
    
    // Workflow tracking
    public DateTime? ApprovedOnUtc { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? RejectedOnUtc { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ReceivedOnUtc { get; private set; }
    public string? ReceivedNotes { get; private set; }
    public DateTime? CompletedOnUtc { get; private set; }
    
    // Return shipping
    public string? ReturnShippingLabel { get; private set; }
    public string? ReturnTrackingNumber { get; private set; }
    public DateTime? ReturnShippedOnUtc { get; private set; }
    
    // Auditing and soft delete are inherited from AuditableEntity
    
    private ReturnRequest()
    {
    }
    
    private ReturnRequest(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new return request.
    /// </summary>
    public static ReturnRequest Create(
        Guid tenantId,
        Guid orderId,
        Guid customerId,
        string reason,
        string? customerNotes,
        int itemCount,
        decimal returnAmount,
        string requestedBy)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID is required", nameof(orderId));
        
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required", nameof(customerId));
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));
        
        if (itemCount <= 0)
            throw new ArgumentException("Item count must be greater than zero", nameof(itemCount));
        
        if (returnAmount <= 0)
            throw new ArgumentException("Return amount must be greater than zero", nameof(returnAmount));
        
        if (string.IsNullOrWhiteSpace(requestedBy))
            throw new ArgumentException("Requested by is required", nameof(requestedBy));
        
        var returnRequest = new ReturnRequest(Guid.NewGuid(), tenantId)
        {
            OrderId = orderId,
            CustomerId = customerId,
            Status = ReturnStatus.Requested,
            RequestedOnUtc = DateTime.UtcNow,
            Reason = reason.Trim(),
            CustomerNotes = customerNotes?.Trim(),
            ItemCount = itemCount,
            ReturnAmount = returnAmount
        };
        
        returnRequest.MarkCreated(DateTime.UtcNow, requestedBy.Trim());
        
        return returnRequest;
    }
    
    /// <summary>
    /// Approve the return request.
    /// </summary>
    public void Approve(string returnShippingLabel, string approvedBy)
    {
        if (Status != ReturnStatus.Requested)
            throw new InvalidOperationException($"Cannot approve return request with status {Status}");
        
        if (string.IsNullOrWhiteSpace(returnShippingLabel))
            throw new ArgumentException("Return shipping label is required", nameof(returnShippingLabel));
        
        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("Approved by is required", nameof(approvedBy));
        
        Status = ReturnStatus.Approved;
        ReturnShippingLabel = returnShippingLabel.Trim();
        ApprovedOnUtc = DateTime.UtcNow;
        ApprovedBy = approvedBy.Trim();
    }
    
    /// <summary>
    /// Reject the return request.
    /// </summary>
    public void Reject(string rejectionReason, string rejectedBy)
    {
        if (Status != ReturnStatus.Requested)
            throw new InvalidOperationException($"Cannot reject return request with status {Status}");
        
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Rejection reason is required", nameof(rejectionReason));
        
        if (string.IsNullOrWhiteSpace(rejectedBy))
            throw new ArgumentException("Rejected by is required", nameof(rejectedBy));
        
        Status = ReturnStatus.Rejected;
        RejectionReason = rejectionReason.Trim();
        RejectedOnUtc = DateTime.UtcNow;
        ApprovedBy = rejectedBy.Trim();
    }
    
    /// <summary>
    /// Record that items have been shipped back.
    /// </summary>
    public void RecordReturnShipment(string trackingNumber)
    {
        if (Status != ReturnStatus.Approved)
            throw new InvalidOperationException($"Cannot record return shipment with status {Status}");
        
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required", nameof(trackingNumber));
        
        ReturnTrackingNumber = trackingNumber.Trim();
        ReturnShippedOnUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Record that items have been received.
    /// </summary>
    public void ReceiveReturnItems(string? notes = null)
    {
        if (Status != ReturnStatus.Approved)
            throw new InvalidOperationException($"Cannot receive return items with status {Status}");
        
        Status = ReturnStatus.Received;
        ReceivedOnUtc = DateTime.UtcNow;
        ReceivedNotes = notes?.Trim();
    }
    
    /// <summary>
    /// Mark return request as in inspection.
    /// </summary>
    public void MarkAsInInspection()
    {
        if (Status != ReturnStatus.Received)
            throw new InvalidOperationException($"Cannot mark as in inspection with status {Status}");
        
        Status = ReturnStatus.InInspection;
    }
    
    /// <summary>
    /// Complete the return process (after inspection and refund).
    /// </summary>
    public void Complete()
    {
        if (Status != ReturnStatus.InInspection)
            throw new InvalidOperationException($"Cannot complete return with status {Status}");
        
        Status = ReturnStatus.Completed;
        CompletedOnUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Cancel the return request.
    /// </summary>
    public void Cancel()
    {
        if (Status == ReturnStatus.Completed || Status == ReturnStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel return request with status {Status}");
        
        Status = ReturnStatus.Cancelled;
    }
    
    /// <summary>
    /// Check if return can be approved.
    /// </summary>
    public bool CanApprove() => Status == ReturnStatus.Requested;
    
    /// <summary>
    /// Check if return can be rejected.
    /// </summary>
    public bool CanReject() => Status == ReturnStatus.Requested;
    
    /// <summary>
    /// Check if return can receive items.
    /// </summary>
    public bool CanReceiveItems() => Status == ReturnStatus.Approved;
}
