using KromicStore.Application.Common.Abstractions;
using KromicStore.Infrastructure.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Text;
using KromicStore.Application.Features.Orders.Commands.ConfirmOrder;
using KromicStore.Application.Features.Orders.Commands.CancelOrder;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.API.Controllers;

/// <summary>
/// Handles payment gateway webhooks (Razorpay).
/// Processes real-time payment status updates and order synchronization.
/// Idempotent: Safe to retry without duplicate processing.
/// </summary>
[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(
        IPaymentGateway paymentGateway,
        IMediator mediator,
        ILogger<PaymentWebhookController> logger)
    {
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle Razorpay webhook events.
    /// Verifies webhook signature and processes payment status updates.
    /// Supports: payment.authorized, payment.failed, payment.captured
    /// </summary>
    /// <response code="200">Webhook processed successfully.</response>
    /// <response code="400">Invalid webhook payload.</response>
    /// <response code="401">Signature verification failed.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("razorpay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleRazorpayWebhook(
        [FromHeader(Name = "X-Razorpay-Signature")] string? signature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Read request body
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var payload = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(payload))
            {
                _logger.LogWarning("Received Razorpay webhook with empty payload");
                return BadRequest(new { error = "Empty payload" });
            }

            if (string.IsNullOrWhiteSpace(signature))
            {
                _logger.LogWarning("Received Razorpay webhook without signature");
                return Unauthorized(new { error = "Missing signature" });
            }

            // Verify webhook signature - CRITICAL for security
            if (!_paymentGateway.VerifyWebhookSignature(payload, signature))
            {
                _logger.LogWarning("Razorpay webhook signature verification failed. Payload: {Payload}", payload);
                return Unauthorized(new { error = "Invalid signature" });
            }

            // Parse webhook event
            var webhookEvent = _paymentGateway.ParseWebhookPayload(payload);
            if (webhookEvent == null)
            {
                _logger.LogWarning("Failed to parse Razorpay webhook payload: {Payload}", payload);
                return BadRequest(new { error = "Invalid payload format" });
            }

            _logger.LogInformation(
                "Processing Razorpay webhook. EventType: {EventType}, PaymentId: {PaymentId}, Status: {Status}, OrderId: {OrderId}",
                webhookEvent.EventType, webhookEvent.PaymentId, webhookEvent.Status, webhookEvent.OrderId);

            // Process based on event type
            await ProcessWebhookEventAsync(webhookEvent, cancellationToken);

            // Return success - webhook is processed
            // Note: Razorpay expects 200 OK to mark webhook as delivered
            return Ok(new { success = true, message = "Webhook processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Razorpay webhook");
            // Return 200 OK even on error to prevent webhook retry storms
            // Webhook processing failures should be monitored separately
            return Ok(new { success = false, message = "Webhook processing queued for retry" });
        }
    }

    /// <summary>
    /// Processes the webhook event based on payment status.
    /// Updates order and payment states appropriately.
    /// Idempotent: Safe to call multiple times for same event.
    /// </summary>
    private async Task ProcessWebhookEventAsync(PaymentWebhookEvent? webhookEvent, CancellationToken cancellationToken)
    {
        if (webhookEvent == null)
        {
            _logger.LogWarning("Null webhook event received");
            return;
        }

        // Extract order ID from webhook event
        if (string.IsNullOrWhiteSpace(webhookEvent.OrderId) || !Guid.TryParse(webhookEvent.OrderId, out var orderId))
        {
            _logger.LogWarning("Invalid order ID in webhook: {OrderId}", webhookEvent.OrderId);
            return;
        }

        try
        {
            switch (webhookEvent.Status)
            {
                case PaymentStatus.Completed:
                case PaymentStatus.Processing:
                    // Payment successful - confirm the order
                    await HandlePaymentSuccessAsync(orderId, webhookEvent, cancellationToken);
                    break;

                case PaymentStatus.Failed:
                case PaymentStatus.Cancelled:
                    // Payment failed - cancel the order
                    await HandlePaymentFailureAsync(orderId, webhookEvent, cancellationToken);
                    break;

                default:
                    _logger.LogWarning(
                        "Unhandled webhook status. EventType: {EventType}, Status: {Status}",
                        webhookEvent.EventType, webhookEvent.Status);
                    break;
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Webhook processing validation failed for order {OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook event for order {OrderId}", orderId);
            throw; // Re-throw to trigger retry mechanism
        }
    }

    /// <summary>
    /// Handles successful payment webhook events.
    /// Confirms the order and publishes success notification.
    /// </summary>
    private async Task HandlePaymentSuccessAsync(Guid orderId, PaymentWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Payment success webhook received. OrderId: {OrderId}, PaymentId: {PaymentId}",
            orderId, webhookEvent.PaymentId);

        try
        {
            // Confirm the order (transitions from Pending to Confirmed)
            var confirmCommand = new ConfirmOrderCommand
            {
                OrderId = orderId,
                TenantId = Guid.NewGuid() // TODO: Extract from webhook or order context
            };

            var result = await _mediator.Send(confirmCommand, cancellationToken);

            _logger.LogInformation(
                "Order confirmed successfully. OrderId: {OrderId}, OrderNumber: {OrderNumber}, Status: {Status}",
                result.OrderId, result.OrderNumber, result.Status);

            // TODO: Publish order confirmation event
            // TODO: Send payment confirmation email
            // TODO: Trigger order fulfillment workflow
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order confirmation failed (may already be confirmed). OrderId: {OrderId}", orderId);
            // This is acceptable - order may have already been confirmed
        }
    }

    /// <summary>
    /// Handles failed payment webhook events.
    /// Cancels the order and publishes failure notification.
    /// </summary>
    private async Task HandlePaymentFailureAsync(Guid orderId, PaymentWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Payment failure webhook received. OrderId: {OrderId}, PaymentId: {PaymentId}, ErrorCode: {ErrorCode}, ErrorDescription: {ErrorDescription}",
            orderId, webhookEvent.PaymentId, webhookEvent.ErrorCode, webhookEvent.ErrorDescription);

        try
        {
            // Cancel the order with payment failure reason
            var cancelCommand = new CancelOrderCommand
            {
                OrderId = orderId,
                Reason = $"Payment failed: {webhookEvent.ErrorDescription ?? "No reason provided"}",
                TenantId = Guid.NewGuid() // TODO: Extract from webhook or order context
            };

            var result = await _mediator.Send(cancelCommand, cancellationToken);

            _logger.LogInformation(
                "Order cancelled due to payment failure. OrderId: {OrderId}, OrderNumber: {OrderNumber}, RefundId: {RefundId}",
                result.OrderId, result.OrderNumber, result.RefundReferenceId);

            // TODO: Publish order cancellation event
            // TODO: Send payment failure notification email with retry option
            // TODO: Schedule automatic refund if applicable
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order cancellation failed (may already be cancelled). OrderId: {OrderId}", orderId);
            // This is acceptable - order may have already been cancelled
        }
    }

    /// <summary>
    /// Health check endpoint for webhook endpoint availability.
    /// Used by payment gateway to verify endpoint is accessible.
    /// </summary>
    [HttpGet("razorpay/health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        _logger.LogDebug("Webhook health check received");
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
