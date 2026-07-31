using KromicStore.Domain.Common;

namespace KromicStore.Domain.Orders.Entities;

/// <summary>
/// Payment aggregate root representing a payment transaction.
/// Manages payment lifecycle, retries, and failure handling.
/// Supports multiple payment methods and providers.
/// </summary>
public sealed class Payment : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? Provider { get; private set; } // e.g., "Stripe", "PayPal", "Bank"
    public string? ProviderTransactionId { get; private set; }
    public PaymentStatus Status { get; private set; }
    
    // Amounts
    public decimal Amount { get; private set; }
    public decimal? RefundedAmount { get; private set; }
    public string Currency { get; private set; } = "USD";
    
    // Retry logic
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public DateTime? NextRetryAtUtc { get; private set; }
    
    // Failure info
    public string? FailureReason { get; private set; }
    public string? FailureCode { get; private set; }
    
    // Timestamps
    public DateTime InitiatedOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public DateTime? RefundedOnUtc { get; private set; }
    
    // Relationships
    private readonly List<PaymentTransaction> _transactions = [];
    public IReadOnlyList<PaymentTransaction> Transactions => _transactions.AsReadOnly();
    
    // Auditing
    public DateTime ModifiedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string ModifiedBy { get; private set; } = string.Empty;
    
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }
    
    private Payment()
    {
    }
    
    private Payment(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }
    
    /// <summary>
    /// Create a new payment.
    /// </summary>
    public static Payment Create(
        Guid tenantId,
        Guid orderId,
        Guid customerId,
        string paymentMethod,
        decimal amount,
        string currency = "USD",
        string? provider = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));
        
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(customerId));
        
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("PaymentMethod cannot be empty", nameof(paymentMethod));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a valid ISO 4217 code (3 characters)", nameof(currency));
        
        var payment = new Payment(Guid.NewGuid(), tenantId)
        {
            OrderId = orderId,
            CustomerId = customerId,
            PaymentMethod = paymentMethod.Trim(),
            Provider = provider?.Trim(),
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Status = PaymentStatus.Pending,
            InitiatedOnUtc = DateTime.UtcNow,
            AttemptCount = 0
        };
        
        return payment;
    }
    
    /// <summary>
    /// Initialize payment processing.
    /// </summary>
    public void InitializeProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Can only process pending payments. Current status: {Status}");
        
        Status = PaymentStatus.Processing;
    }
    
    /// <summary>
    /// Mark payment as successfully processed.
    /// </summary>
    public void MarkAsSuccessful(string? providerTransactionId = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Can only succeed processing payments. Current status: {Status}");
        
        Status = PaymentStatus.Completed;
        ProcessedOnUtc = DateTime.UtcNow;
        ProviderTransactionId = providerTransactionId;
        AttemptCount++;
        FailureReason = null;
        FailureCode = null;
    }
    
    /// <summary>
    /// Mark payment as failed and schedule retry if attempts remain.
    /// </summary>
    public void MarkAsFailed(string failureReason, string? failureCode = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Can only fail processing payments. Current status: {Status}");
        
        AttemptCount++;
        FailureReason = failureReason?.Trim();
        FailureCode = failureCode?.Trim();
        
        if (AttemptCount < MaxAttempts)
        {
            // Schedule retry - exponential backoff (2 min, 5 min, 10 min)
            var delayMinutes = (int)Math.Pow(2, AttemptCount);
            NextRetryAtUtc = DateTime.UtcNow.AddMinutes(delayMinutes);
            Status = PaymentStatus.RetryScheduled;
        }
        else
        {
            Status = PaymentStatus.Failed;
        }
    }
    
    /// <summary>
    /// Cancel the payment.
    /// </summary>
    public void Cancel(string reason = "")
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException($"Cannot cancel {Status} payments");
        
        Status = PaymentStatus.Cancelled;
        FailureReason = reason;
    }
    
    /// <summary>
    /// Process refund for the payment.
    /// </summary>
    public void ProcessRefund(decimal refundAmount)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException($"Can only refund completed payments. Current status: {Status}");
        
        if (refundAmount <= 0 || refundAmount > Amount)
            throw new ArgumentException($"Refund amount must be between 0 and {Amount}", nameof(refundAmount));
        
        RefundedAmount = (RefundedAmount ?? 0) + refundAmount;
        
        if (RefundedAmount >= Amount)
        {
            Status = PaymentStatus.Refunded;
        }
        else
        {
            Status = PaymentStatus.PartiallyRefunded;
        }
        
        RefundedOnUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Add a transaction record.
    /// </summary>
    public void AddTransaction(PaymentTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
        
        _transactions.Add(transaction);
    }
    
    /// <summary>
    /// Check if payment can be retried.
    /// </summary>
    public bool CanRetry => Status == PaymentStatus.RetryScheduled && 
                            NextRetryAtUtc.HasValue && 
                            DateTime.UtcNow >= NextRetryAtUtc.Value;
    
    /// <summary>
    /// Get remaining refundable amount.
    /// </summary>
    public decimal GetRemainderForRefund()
    {
        return Amount - (RefundedAmount ?? 0);
    }
}

/// <summary>
/// Payment status enumeration.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,             // Awaiting processing
    Processing = 1,          // Currently processing
    Completed = 2,           // Successfully processed
    Failed = 3,              // Failed after all retries
    RetryScheduled = 4,      // Failed but retry scheduled
    Cancelled = 5,           // Cancelled by user/system
    Refunded = 6,            // Fully refunded
    PartiallyRefunded = 7    // Partially refunded
}
