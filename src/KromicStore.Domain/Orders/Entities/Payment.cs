using KromicStore.Domain.Common;

namespace KromicStore.Domain.Orders.Entities;

/// <summary>
/// Represents a payment transaction with full lifecycle, retry logic, and refund handling.
/// Tenant-scoped aggregate root that supports multiple payment methods and providers.
/// </summary>
public class Payment : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Provider { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public int AttemptCount { get; private set; }
    public const int MaxAttempts = 3;
    public DateTime? NextRetryAtUtc { get; private set; }
    public string? ProviderTransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public decimal RefundedAmount { get; private set; }
    public DateTime? RefundedOnUtc { get; private set; }
    public string? IdempotencyKey { get; private set; }

    private readonly List<PaymentTransaction> _transactions = new();
    public IReadOnlyList<PaymentTransaction> Transactions => _transactions.AsReadOnly();

    // Auditing and soft delete are inherited from AuditableEntity

    private Payment() { }

    private Payment(Guid id, Guid tenantId) : base(id, tenantId) { }

    /// <summary>
    /// Creates a new payment entity for an order.
    /// </summary>
    public static Payment Create(
        Guid tenantId,
        Guid orderId,
        Guid customerId,
        string paymentMethod,
        decimal amount,
        string currency,
        string provider)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID is required.", nameof(orderId));

        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.", nameof(customerId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method is required.", nameof(paymentMethod));

        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required.", nameof(provider));

        var payment = new Payment(Guid.NewGuid(), tenantId)
        {
            OrderId = orderId,
            CustomerId = customerId,
            PaymentMethod = paymentMethod,
            Amount = amount,
            Currency = currency.ToUpper(),
            Provider = provider,
            Status = PaymentStatus.Pending,
            AttemptCount = 0,
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        payment.MarkCreated(DateTime.UtcNow, "system");
        return payment;
    }

    /// <summary>
    /// Transition payment to Processing status.
    /// </summary>
    public void InitializeProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be processed.");

        Status = PaymentStatus.Processing;
        AttemptCount++;
        MarkModified(DateTime.UtcNow, "system");
    }

    /// <summary>
    /// Mark payment as successfully completed.
    /// </summary>
    public void MarkAsSuccessful(string? providerTransactionId = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException("Only processing payments can be marked as successful.");

        Status = PaymentStatus.Completed;
        ProviderTransactionId = providerTransactionId;
        FailureReason = null;
        MarkModified(DateTime.UtcNow, "system");
    }

    /// <summary>
    /// Mark payment as failed and schedule retry if attempts remain.
    /// </summary>
    public void MarkAsFailed(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
            throw new ArgumentException("Failure reason is required.", nameof(failureReason));

        FailureReason = failureReason;

        if (AttemptCount < MaxAttempts)
        {
            Status = PaymentStatus.RetryScheduled;
            NextRetryAtUtc = DateTime.UtcNow.AddSeconds(Math.Pow(2, AttemptCount) * 60); // Exponential backoff
        }
        else
        {
            Status = PaymentStatus.Failed;
        }

        MarkModified(DateTime.UtcNow, "system");
    }

    /// <summary>
    /// Cancel the payment.
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == PaymentStatus.Completed || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot cancel a completed or refunded payment.");

        Status = PaymentStatus.Cancelled;
        FailureReason = reason;
        MarkModified(DateTime.UtcNow, "system");
    }

    /// <summary>
    /// Process a refund (full or partial).
    /// </summary>
    public void ProcessRefund(decimal refundAmount)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded.");

        if (refundAmount <= 0)
            throw new ArgumentException("Refund amount must be greater than 0.", nameof(refundAmount));

        if (RefundedAmount + refundAmount > Amount)
            throw new ArgumentException("Refund amount exceeds available refundable amount.", nameof(refundAmount));

        RefundedAmount += refundAmount;
        RefundedOnUtc = DateTime.UtcNow;

        if (RefundedAmount == Amount)
        {
            Status = PaymentStatus.Refunded;
        }
        else if (RefundedAmount > 0 && RefundedAmount < Amount)
        {
            Status = PaymentStatus.PartiallyRefunded;
        }

        MarkModified(DateTime.UtcNow, "system");
    }

    /// <summary>
    /// Get the remaining amount available for refund.
    /// </summary>
    public decimal GetRemainderForRefund()
    {
        return Amount - RefundedAmount;
    }

    /// <summary>
    /// Add a transaction record to this payment.
    /// </summary>
    public void AddTransaction(PaymentTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");

        _transactions.Add(transaction);
        MarkModified(DateTime.UtcNow, "system");
    }
}

/// <summary>
/// Payment status enumeration.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    RetryScheduled = 5,
    Refunded = 6,
    PartiallyRefunded = 7
}

/// <summary>
/// Payment method enumeration for gateway results.
/// </summary>
public enum PaymentMethod
{
    Unknown = 0,
    CreditCard = 1,
    DebitCard = 2,
    NetBanking = 3,
    UPI = 4,
    Wallet = 5,
    EMI = 6
}
