using KromicStore.Application.Features.Orders.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Services.Payments;

/// <summary>
/// Refund service that delegates to payment gateway implementations.
/// Provides abstraction for vendor-agnostic refund processing.
/// </summary>
public sealed class RefundService : IRefundService
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<RefundService> _logger;

    public RefundService(IPaymentGateway paymentGateway, ILogger<RefundService> logger)
    {
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> RefundPaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        decimal? refundAmount = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalPaymentId))
        {
            throw new ArgumentException("External payment ID is required", nameof(externalPaymentId));
        }

        _logger.LogInformation(
            "Processing refund. TenantId: {TenantId}, PaymentId: {PaymentId}, Amount: {Amount}, Reason: {Reason}",
            tenantId, externalPaymentId, refundAmount ?? 0, reason ?? "Not specified");

        try
        {
            // Delegate to payment gateway
            var refundResult = await _paymentGateway.RefundPaymentAsync(
                tenantId,
                externalPaymentId,
                refundAmount,
                reason,
                cancellationToken: cancellationToken);

            if (!refundResult.Success)
            {
                _logger.LogError(
                    "Refund failed. PaymentId: {PaymentId}, Error: {Error}",
                    externalPaymentId, refundResult.ErrorMessage);

                throw new InvalidOperationException(
                    $"Refund processing failed: {refundResult.ErrorMessage}");
            }

            if (string.IsNullOrWhiteSpace(refundResult.RefundId))
            {
                _logger.LogError(
                    "Refund succeeded but no refund ID was returned. PaymentId: {PaymentId}",
                    externalPaymentId);

                throw new InvalidOperationException("Refund succeeded but refund ID was not returned by payment gateway");
            }

            _logger.LogInformation(
                "Refund processed successfully. PaymentId: {PaymentId}, RefundId: {RefundId}, Amount: {Amount}",
                externalPaymentId, refundResult.RefundId, refundResult.RefundAmount);

            return refundResult.RefundId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during refund processing. PaymentId: {PaymentId}", externalPaymentId);
            throw;
        }
    }
}
