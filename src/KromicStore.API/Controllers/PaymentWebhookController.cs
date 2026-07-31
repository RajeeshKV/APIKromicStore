using KromicStore.Application.Common.Abstractions;
using KromicStore.Infrastructure.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;

namespace KromicStore.API.Controllers;

/// <summary>
/// Handles payment gateway webhooks (Razorpay).
/// Processes real-time payment status updates and order synchronization.
/// </summary>
[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(
        IPaymentGateway paymentGateway,
        ILogger<PaymentWebhookController> logger)
    {
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle Razorpay webhook events.
    /// Verifies webhook signature and processes payment status updates.
    /// </summary>
    [HttpPost("razorpay")]
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
                return BadRequest("Empty payload");
            }

            if (string.IsNullOrWhiteSpace(signature))
            {
                _logger.LogWarning("Received Razorpay webhook without signature");
                return Unauthorized("Missing signature");
            }

            // Verify webhook signature
            if (!_paymentGateway.VerifyWebhookSignature(payload, signature))
            {
                _logger.LogWarning("Razorpay webhook signature verification failed");
                return Unauthorized("Invalid signature");
            }

            // Parse webhook event
            var webhookEvent = _paymentGateway.ParseWebhookPayload(payload);
            if (webhookEvent == null)
            {
                _logger.LogWarning("Failed to parse Razorpay webhook payload");
                return BadRequest("Invalid payload format");
            }

            _logger.LogInformation(
                "Processing Razorpay webhook. EventType: {EventType}, PaymentId: {PaymentId}, Status: {Status}",
                webhookEvent.EventType, webhookEvent.PaymentId, webhookEvent.Status);

            // For now, just log the event and return success
            // In production, you would:
            // 1. Dispatch UpdatePaymentStatusCommand
            // 2. Dispatch UpdateOrderStatusCommand based on payment status
            // 3. Handle other business logic

            return Ok(new { success = true, message = "Webhook processed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Razorpay webhook");
            return StatusCode(500, "Internal server error");
        }
    }
}
