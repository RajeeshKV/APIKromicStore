using KromicStore.Domain.Common;

namespace KromicStore.Domain.Orders.Entities;

/// <summary>
/// PaymentTransaction value object representing a single transaction attempt.
/// Maintains a record of all payment processing attempts.
/// </summary>
public sealed class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public string TransactionType { get; private set; } = string.Empty; // "Authorization", "Capture", "Refund", "Retry"
    public string? ProviderTransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Status { get; private set; } = string.Empty; // "Success", "Failed", "Pending"
    public string? ResponseCode { get; private set; }
    public string? ResponseMessage { get; private set; }
    public string? RawResponse { get; private set; } // Store full provider response for debugging
    public DateTime CreatedOnUtc { get; private set; }
    
    private PaymentTransaction()
    {
    }
    
    private PaymentTransaction(Guid id) : base(id)
    {
    }
    
    /// <summary>
    /// Create a successful transaction record.
    /// </summary>
    public static PaymentTransaction CreateSuccess(
        Guid paymentId,
        string transactionType,
        decimal amount,
        string? providerTransactionId = null,
        string? responseCode = null,
        string? responseMessage = null,
        string? rawResponse = null)
    {
        return Create(
            paymentId,
            transactionType,
            amount,
            "Success",
            providerTransactionId,
            responseCode,
            responseMessage,
            rawResponse);
    }
    
    /// <summary>
    /// Create a failed transaction record.
    /// </summary>
    public static PaymentTransaction CreateFailure(
        Guid paymentId,
        string transactionType,
        decimal amount,
        string failureReason,
        string? responseCode = null,
        string? rawResponse = null)
    {
        return Create(
            paymentId,
            transactionType,
            amount,
            "Failed",
            null,
            responseCode,
            failureReason,
            rawResponse);
    }
    
    /// <summary>
    /// Create a pending transaction record (for async processing).
    /// </summary>
    public static PaymentTransaction CreatePending(
        Guid paymentId,
        string transactionType,
        decimal amount,
        string? responseCode = null)
    {
        return Create(
            paymentId,
            transactionType,
            amount,
            "Pending",
            null,
            responseCode,
            null,
            null);
    }
    
    /// <summary>
    /// Internal factory method.
    /// </summary>
    private static PaymentTransaction Create(
        Guid paymentId,
        string transactionType,
        decimal amount,
        string status,
        string? providerTransactionId = null,
        string? responseCode = null,
        string? responseMessage = null,
        string? rawResponse = null)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentException("PaymentId cannot be empty", nameof(paymentId));
        
        if (string.IsNullOrWhiteSpace(transactionType))
            throw new ArgumentException("TransactionType cannot be empty", nameof(transactionType));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be empty", nameof(status));
        
        return new PaymentTransaction(Guid.NewGuid())
        {
            PaymentId = paymentId,
            TransactionType = transactionType.Trim(),
            ProviderTransactionId = providerTransactionId?.Trim(),
            Amount = amount,
            Status = status.Trim(),
            ResponseCode = responseCode?.Trim(),
            ResponseMessage = responseMessage?.Trim(),
            RawResponse = rawResponse,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
}
