namespace KromicStore.Application.Features.Orders.Abstractions;

/// <summary>
/// Abstraction for refund processing across payment gateways.
/// Enables vendor-agnostic refund handling from the application layer.
/// </summary>
public interface IRefundService
{
    /// <summary>
    /// Initiates a refund for a captured payment.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="externalPaymentId">External payment ID from payment gateway.</param>
    /// <param name="refundAmount">Amount to refund (null for full refund).</param>
    /// <param name="reason">Reason for refund.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Refund reference ID if successful, throws on failure.</returns>
    Task<string> RefundPaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        decimal? refundAmount = null,
        string? reason = null,
        CancellationToken cancellationToken = default);
}
