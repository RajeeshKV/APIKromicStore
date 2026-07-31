using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Infrastructure.Services.Payments;

/// <summary>
/// Refund service for processing payment refunds.
/// Provides abstraction over payment gateway refund operations.
/// </summary>
public sealed class RefundService : IRefundService
{
    private const int MaxRetryAttempts = 3;
    private const int InitialRetryDelayMs = 100;

    public async Task<string> RefundPaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        decimal? refundAmount = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalPaymentId, nameof(externalPaymentId));

        return await ProcessRefundWithRetryAsync(
            tenantId,
            externalPaymentId,
            refundAmount,
            reason ?? "Refund requested",
            cancellationToken);
    }

    private async Task<string> ProcessRefundWithRetryAsync(
        Guid tenantId,
        string externalPaymentId,
        decimal? refundAmount,
        string reason,
        CancellationToken cancellationToken)
    {
        int retryCount = 0;
        int delayMs = InitialRetryDelayMs;

        while (true)
        {
            try
            {
                // Process refund - in production, this would call the payment gateway
                // For now, we generate a deterministic refund ID based on input
                var refundId = GenerateRefundId(externalPaymentId, tenantId);

                return refundId;
            }
            catch (Exception ex) when (retryCount < MaxRetryAttempts && IsRetryableError(ex))
            {
                retryCount++;
                await Task.Delay(delayMs, cancellationToken);
                delayMs = (int)(delayMs * 1.5); // Exponential backoff
            }
        }
    }

    private static bool IsRetryableError(Exception ex)
    {
        // Retry on timeout or connection errors
        return ex is TimeoutException ||
               ex is HttpRequestException ||
               (ex.InnerException is TimeoutException) ||
               (ex.InnerException is HttpRequestException);
    }

    private static string GenerateRefundId(string paymentId, Guid tenantId)
    {
        return $"refund_{paymentId}_{tenantId:N}";
    }
}
