using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KromicStore.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KromicStore.Infrastructure.Services.Payments;

/// <summary>
/// Razorpay payment gateway implementation.
/// Provides payment processing, capture, refund, and webhook verification.
/// </summary>
public class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RazorpayOptions _options;
    private readonly ILogger<RazorpayPaymentGateway> _logger;

    public RazorpayPaymentGateway(
        IHttpClientFactory httpClientFactory,
        IOptions<RazorpayOptions> options,
        ILogger<RazorpayPaymentGateway> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentCreateResult> CreatePaymentAsync(
        Guid tenantId,
        Guid orderId,
        decimal amount,
        string currency,
        string customerEmail,
        string customerName,
        Dictionary<string, string>? metadata = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID is required.", nameof(orderId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        if (!_options.IsCurrencyAllowed(currency))
            throw new ArgumentException($"Currency {currency} is not allowed.", nameof(currency));

        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email is required.", nameof(customerEmail));

        try
        {
            _logger.LogInformation(
                "Creating Razorpay payment for tenant {TenantId}, order {OrderId}, amount {Amount} {Currency}",
                tenantId, orderId, amount, currency);

            var amountInPaise = (long)(amount * 100);

            var payload = new
            {
                amount = amountInPaise,
                currency = currency.ToUpperInvariant(),
                customer_notify = 1,
                receipt = orderId.ToString().Substring(0, 40),
                email = customerEmail,
                contact = "1112220061",
                notes = metadata ?? new Dictionary<string, string> { ["orderId"] = orderId.ToString() }
            };

            var client = _httpClientFactory.CreateClient("Razorpay");
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                content.Headers.Add("Idempotency-Key", idempotencyKey);

            var response = await client.PostAsync(
                "/orders",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Razorpay payment creation failed for order {OrderId}. Status: {StatusCode}, Error: {Error}",
                    orderId, response.StatusCode, errorContent);

                return new PaymentCreateResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = "Failed to create payment order"
                };
            }

            var razorpayResponseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<RazorpayOrderResponse>(razorpayResponseString);

            _logger.LogInformation(
                "Razorpay payment order created successfully for order {OrderId}. OrderId: {RazorpayOrderId}",
                orderId, responseData?.Id);

            return new PaymentCreateResult
            {
                Success = true,
                ExternalPaymentId = responseData?.Id,
                Amount = amount,
                Currency = currency,
                OrderId = orderId.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while creating Razorpay payment for order {OrderId}",
                orderId);

            return new PaymentCreateResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentCaptureResult> CapturePaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalPaymentId))
            throw new ArgumentException("External payment ID is required.", nameof(externalPaymentId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.", nameof(amount));

        try
        {
            _logger.LogInformation(
                "Capturing Razorpay payment {PaymentId} for tenant {TenantId}, amount {Amount}",
                externalPaymentId, tenantId, amount);

            var amountInPaise = (long)(amount * 100);

            var payload = new { amount = amountInPaise };

            var client = _httpClientFactory.CreateClient("Razorpay");
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"/payments/{externalPaymentId}/capture",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Razorpay payment capture failed for payment {PaymentId}. Status: {StatusCode}, Error: {Error}",
                    externalPaymentId, response.StatusCode, errorContent);

                return new PaymentCaptureResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = "Failed to capture payment"
                };
            }

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<RazorpayPaymentResponse>(responseString);

            _logger.LogInformation(
                "Razorpay payment {PaymentId} captured successfully. Status: {Status}",
                externalPaymentId, responseData?.Status);

            return new PaymentCaptureResult
            {
                Success = true,
                ExternalPaymentId = externalPaymentId,
                Amount = amount,
                Status = ParsePaymentStatus(responseData?.Status)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while capturing Razorpay payment {PaymentId}",
                externalPaymentId);

            return new PaymentCaptureResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentCancelResult> CancelPaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalPaymentId))
            throw new ArgumentException("External payment ID is required.", nameof(externalPaymentId));

        try
        {
            _logger.LogInformation(
                "Cancelling Razorpay payment {PaymentId} for tenant {TenantId}",
                externalPaymentId, tenantId);

            var client = _httpClientFactory.CreateClient("Razorpay");

            var response = await client.PostAsync(
                $"/payments/{externalPaymentId}/cancel",
                new StringContent("", Encoding.UTF8, "application/json"),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Razorpay payment cancellation failed for payment {PaymentId}. Status: {StatusCode}",
                    externalPaymentId, response.StatusCode);

                return new PaymentCancelResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = "Failed to cancel payment"
                };
            }

            _logger.LogInformation("Razorpay payment {PaymentId} cancelled successfully", externalPaymentId);

            return new PaymentCancelResult
            {
                Success = true,
                ExternalPaymentId = externalPaymentId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while cancelling Razorpay payment {PaymentId}",
                externalPaymentId);

            return new PaymentCancelResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<RefundResult> RefundPaymentAsync(
        Guid tenantId,
        string externalPaymentId,
        decimal? refundAmount = null,
        string? reason = null,
        string? receiptId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalPaymentId))
            throw new ArgumentException("External payment ID is required.", nameof(externalPaymentId));

        try
        {
            _logger.LogInformation(
                "Creating Razorpay refund for payment {PaymentId} for tenant {TenantId}, amount {Amount}",
                externalPaymentId, tenantId, refundAmount);

            var payload = new Dictionary<string, object>
            {
                ["notes"] = new { reason = reason ?? "Customer requested refund" }
            };

            if (refundAmount.HasValue && refundAmount > 0)
            {
                payload["amount"] = (long)(refundAmount * 100);
            }

            if (!string.IsNullOrWhiteSpace(receiptId))
            {
                payload["receipt"] = receiptId;
            }

            var client = _httpClientFactory.CreateClient("Razorpay");
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"/payments/{externalPaymentId}/refund",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Razorpay refund failed for payment {PaymentId}. Status: {StatusCode}, Error: {Error}",
                    externalPaymentId, response.StatusCode, errorContent);

                return new RefundResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = "Failed to create refund"
                };
            }

            var refundResponseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<RazorpayRefundResponse>(refundResponseString);

            _logger.LogInformation(
                "Razorpay refund created successfully for payment {PaymentId}. RefundId: {RefundId}",
                externalPaymentId, responseData?.Id);

            return new RefundResult
            {
                Success = true,
                ExternalPaymentId = externalPaymentId,
                RefundId = responseData?.Id,
                RefundAmount = refundAmount ?? 0,
                Status = responseData?.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while creating Razorpay refund for payment {PaymentId}",
                externalPaymentId);

            return new RefundResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return false;

        if (string.IsNullOrWhiteSpace(signature))
            return false;

        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.WebhookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            return computedSignature == signature.ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Razorpay webhook signature");
            return false;
        }
    }

    public PaymentWebhookEvent? ParseWebhookPayload(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (!root.TryGetProperty("event", out var eventElement))
                return null;

            var eventType = eventElement.GetString();
            if (string.IsNullOrWhiteSpace(eventType))
                return null;

            if (!root.TryGetProperty("payload", out var payloadElement))
                return null;

            if (!payloadElement.TryGetProperty("payment", out var paymentElement))
                return null;

            var paymentId = paymentElement.GetProperty("entity").GetProperty("id").GetString();
            var status = paymentElement.GetProperty("entity").GetProperty("status").GetString();
            var amount = paymentElement.GetProperty("entity").GetProperty("amount").GetInt64();

            return new PaymentWebhookEvent
            {
                EventId = paymentId ?? string.Empty,
                EventType = eventType,
                PaymentId = paymentId,
                Status = ParsePaymentStatus(status),
                Amount = amount / 100m
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Razorpay webhook payload");
            return null;
        }
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Razorpay");
            var response = await client.GetAsync("/payments", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay health check failed");
            return false;
        }
    }

    private Domain.Orders.Entities.PaymentStatus ParsePaymentStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "authorized" => Domain.Orders.Entities.PaymentStatus.Processing,
        "captured" => Domain.Orders.Entities.PaymentStatus.Completed,
        "failed" => Domain.Orders.Entities.PaymentStatus.Failed,
        "refunded" => Domain.Orders.Entities.PaymentStatus.Refunded,
        _ => Domain.Orders.Entities.PaymentStatus.Pending
    };
}

internal class RazorpayOrderResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string? Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("amount")]
    public long Amount { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("currency")]
    public string? Currency { get; set; }
}

internal class RazorpayPaymentResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string? Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string? Status { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("amount")]
    public long Amount { get; set; }
}

internal class RazorpayRefundResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string? Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string? Status { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("amount")]
    public long Amount { get; set; }
}
