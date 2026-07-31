using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Infrastructure.Services.Payments;

/// <summary>
/// Abstraction for payment gateway providers (Razorpay, Stripe, etc.).
/// Enables vendor-agnostic payment processing with multi-tenant support.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Create a payment order for checkout.
    /// </summary>
    Task<PaymentCreateResult> CreatePaymentAsync(
        Guid tenantId,
        Guid orderId,
        decimal amount,
        string currency,
        string customerEmail,
        string customerName,
        Dictionary<string, string>? metadata = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Capture an authorized payment.
    /// </summary>
    Task<PaymentCaptureResult> CapturePaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        decimal amount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a pending payment.
    /// </summary>
    Task<PaymentCancelResult> CancelPaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund a captured payment (full or partial).
    /// </summary>
    Task<RefundResult> RefundPaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        decimal? refundAmount = null,
        string? reason = null,
        string? receiptId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify webhook signature from payment provider.
    /// </summary>
    bool VerifyWebhookSignature(string payload, string signature);

    /// <summary>
    /// Parse webhook payload into payment event.
    /// </summary>
    PaymentWebhookEvent? ParseWebhookPayload(string payload);

    /// <summary>
    /// Health check - verify service connectivity.
    /// </summary>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of payment creation.
/// </summary>
public class PaymentCreateResult
{
    public bool Success { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? PaymentUrl { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? OrderId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Result of payment capture.
/// </summary>
public class PaymentCaptureResult
{
    public bool Success { get; set; }
    public string? ExternalPaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of payment cancellation.
/// </summary>
public class PaymentCancelResult
{
    public bool Success { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of refund operation.
/// </summary>
public class RefundResult
{
    public bool Success { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? RefundId { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string? Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Webhook event from payment provider.
/// </summary>
public class PaymentWebhookEvent
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? PaymentId { get; set; }
    public string? OrderId { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentMethod? Method { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDescription { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
}
