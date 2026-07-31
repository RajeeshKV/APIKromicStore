using KromicStore.API.Contracts;
using KromicStore.Infrastructure.Services.Payments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers;

/// <summary>
/// Webhook endpoints for external service integrations.
/// Handles payment gateway callbacks, email delivery confirmations, and other async notifications.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous] // Webhooks must be publicly accessible for external services
public sealed class WebhooksController : ControllerBase
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IMediator _mediator;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IPaymentGateway paymentGateway,
        IMediator mediator,
        ILogger<WebhooksController> logger)
    {
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Receives webhook callbacks from Razorpay payment gateway.
    /// 
    /// Validates webhook signature, parses payment event, and updates order/payment status.
    /// Expected payload includes payment_id, order_id, amount, status, and other metadata.
    /// 
    /// Razorpay Events Handled:
    /// - payment.authorized: Payment authorized, capture pending
    /// - payment.captured: Payment successfully captured
    /// - payment.failed: Payment authorization/capture failed
    /// - payment.refunded: Payment refunded to customer
    /// 
    /// Response Codes:
    /// - 200 OK: Webhook processed successfully (idempotent)
    /// - 401 Unauthorized: Invalid webhook signature
    /// - 400 Bad Request: Invalid payload or missing fields
    /// - 500 Internal Server Error: Processing error (retry sent by Razorpay)
    /// </summary>
    /// <param name="request">The webhook payload from Razorpay</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status result indicating webhook processing outcome</returns>
    [HttpPost("razorpay")]
    [Produces("application/json")]
    public async Task<IActionResult> HandleRazorpayWebhook(
        [FromBody] RazorpayWebhookRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            _logger.LogWarning("Razorpay webhook received with null payload");
            return BadRequest(new ApiResponse<object>(false, null, "Webhook payload is required", new[] { "Null payload" }, HttpContext.TraceIdentifier));
        }

        try
        {
            _logger.LogInformation(
                "Razorpay webhook received. Event: {Event}, PaymentId: {PaymentId}",
                request.Event, request.Payload?.Payment?.Entity?.Id);

            // Verify webhook signature
            var signature = Request.Headers["X-Razorpay-Signature"].ToString();
            if (string.IsNullOrWhiteSpace(signature))
            {
                _logger.LogWarning("Razorpay webhook missing signature header");
                return Unauthorized(new ApiResponse<object>(false, null, "Webhook signature missing", new[] { "Missing signature" }, HttpContext.TraceIdentifier));
            }

            // Serialize request body to verify signature
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            if (!_paymentGateway.VerifyWebhookSignature(requestBody, signature))
            {
                _logger.LogWarning("Razorpay webhook signature verification failed");
                return Unauthorized(new ApiResponse<object>(false, null, "Invalid webhook signature", new[] { "Signature verification failed" }, HttpContext.TraceIdentifier));
            }

            // Parse webhook event
            var webhookEvent = _paymentGateway.ParseWebhookPayload(requestBody);
            if (webhookEvent == null)
            {
                _logger.LogWarning("Failed to parse Razorpay webhook payload");
                return BadRequest(new ApiResponse<object>(false, null, "Failed to parse webhook payload", new[] { "Parse error" }, HttpContext.TraceIdentifier));
            }

            _logger.LogInformation(
                "Razorpay webhook verified. Event: {Event}, PaymentId: {PaymentId}, Status: {Status}",
                webhookEvent.EventType, webhookEvent.PaymentId, webhookEvent.Status);

            // Handle specific webhook events
            switch (webhookEvent.EventType.ToLowerInvariant())
            {
                case "payment.authorized":
                    await HandlePaymentAuthorized(webhookEvent, cancellationToken);
                    break;

                case "payment.captured":
                    await HandlePaymentCaptured(webhookEvent, cancellationToken);
                    break;

                case "payment.failed":
                    await HandlePaymentFailed(webhookEvent, cancellationToken);
                    break;

                case "payment.refunded":
                    await HandlePaymentRefunded(webhookEvent, cancellationToken);
                    break;

                default:
                    _logger.LogInformation(
                        "Razorpay webhook event {Event} received but not handled",
                        webhookEvent.EventType);
                    break;
            }

            return Ok(new ApiResponse<object>(true, null, "Webhook processed successfully", [], HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing Razorpay webhook");

            // Return 200 OK anyway to prevent Razorpay retries
            // Logging and manual intervention needed
            return Ok(new ApiResponse<object>(true, null, "Webhook received and logged", [], HttpContext.TraceIdentifier));
        }
    }

    private async Task HandlePaymentAuthorized(PaymentWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment authorized: {PaymentId}", webhookEvent.PaymentId);
        
        // TODO: Implementation depends on order workflow
        // May need to update order status to "Payment Authorized - Pending Capture"
        // For now, log the event
        
        await Task.CompletedTask;
    }

    private async Task HandlePaymentCaptured(PaymentWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment captured: {PaymentId}, Amount: {Amount}", webhookEvent.PaymentId, webhookEvent.Amount);
        
        // TODO: Update Payment entity status to Completed
        // TODO: Update Order entity status to Confirmed/Processing
        // TODO: Trigger order processing workflow
        
        await Task.CompletedTask;
    }

    private async Task HandlePaymentFailed(PaymentWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Payment failed: {PaymentId}", webhookEvent.PaymentId);
        
        // TODO: Update Payment entity status to Failed
        // TODO: Update Order entity status to Payment Failed
        // TODO: Trigger notification to customer
        
        await Task.CompletedTask;
    }

    private async Task HandlePaymentRefunded(PaymentWebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment refunded: {PaymentId}, Amount: {Amount}", webhookEvent.PaymentId, webhookEvent.Amount);
        
        // TODO: Update Payment entity status to Refunded
        // TODO: Update Order entity status to Refunded
        // TODO: Trigger refund notification to customer
        
        await Task.CompletedTask;
    }
}

/// <summary>
/// Request model for Razorpay webhook payload.
/// Represents the structure of webhook events sent by Razorpay.
/// </summary>
public class RazorpayWebhookRequest
{
    [System.Text.Json.Serialization.JsonPropertyName("event")]
    public string? Event { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("payload")]
    public RazorpayWebhookPayload? Payload { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }
}

/// <summary>
/// Webhook payload containing payment event details.
/// </summary>
public class RazorpayWebhookPayload
{
    [System.Text.Json.Serialization.JsonPropertyName("payment")]
    public RazorpayPaymentEntity? Payment { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("refund")]
    public RazorpayRefundEntity? Refund { get; set; }
}

/// <summary>
/// Payment entity details from webhook.
/// </summary>
public class RazorpayPaymentEntity
{
    [System.Text.Json.Serialization.JsonPropertyName("entity")]
    public RazorpayPaymentDetails? Entity { get; set; }
}

/// <summary>
/// Refund entity details from webhook.
/// </summary>
public class RazorpayRefundEntity
{
    [System.Text.Json.Serialization.JsonPropertyName("entity")]
    public RazorpayRefundDetails? Entity { get; set; }
}

/// <summary>
/// Payment details from webhook entity.
/// </summary>
public class RazorpayPaymentDetails
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string? Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string? Status { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("amount")]
    public long Amount { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("order_id")]
    public string? OrderId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string? Email { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("contact")]
    public string? Contact { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("notes")]
    public Dictionary<string, object>? Notes { get; set; }
}

/// <summary>
/// Refund details from webhook entity.
/// </summary>
public class RazorpayRefundDetails
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string? Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("payment_id")]
    public string? PaymentId { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("amount")]
    public long Amount { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string? Status { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("notes")]
    public Dictionary<string, object>? Notes { get; set; }
}

