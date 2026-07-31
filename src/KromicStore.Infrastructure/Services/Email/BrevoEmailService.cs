using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KromicStore.Domain.Email.Entities;
using KromicStore.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KromicStore.Infrastructure.Services.Email;

/// <summary>
/// Brevo email service implementation.
/// Provides email sending with template support, signature verification, and error handling.
/// </summary>
public class BrevoEmailService : IEmailService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BrevoOptions _options;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        IHttpClientFactory httpClientFactory,
        IOptions<BrevoOptions> options,
        ILogger<BrevoEmailService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EmailSendResult> SendTemplateEmailAsync(
        Guid tenantId,
        string recipientEmail,
        string recipientName,
        string templateId,
        Dictionary<string, string> variables,
        Dictionary<string, string>? customHeaders = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email is required.", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID is required.", nameof(templateId));

        try
        {
            _logger.LogInformation(
                "Sending Brevo template email to {RecipientEmail} for tenant {TenantId} using template {TemplateId}",
                recipientEmail, tenantId, templateId);

            var client = _httpClientFactory.CreateClient("Brevo");
            
            var payload = new Dictionary<string, object>
            {
                { "to", new[] { new { email = recipientEmail, name = recipientName } } },
                { "templateId", long.Parse(templateId) },
                { "params", variables }
            };
            
            if (customHeaders != null)
            {
                payload["headers"] = customHeaders;
            }

            var response = await client.PostAsJsonAsync(
                "/smtp/email",
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Brevo email send failed for {RecipientEmail}. Status: {StatusCode}, Error: {Error}",
                    recipientEmail, response.StatusCode, errorContent);

                return new EmailSendResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = "Failed to send email through Brevo"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDoc = JsonSerializer.Deserialize<BrevEmailResponse>(responseContent);
            
            _logger.LogInformation(
                "Brevo email sent successfully to {RecipientEmail}. MessageId: {MessageId}",
                recipientEmail, jsonDoc?.MessageId);

            return new EmailSendResult
            {
                Success = true,
                ExternalMessageId = jsonDoc?.MessageId.ToString(),
                SentAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while sending Brevo email to {RecipientEmail}",
                recipientEmail);

            return new EmailSendResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<EmailSendResult> SendRawEmailAsync(
        Guid tenantId,
        string recipientEmail,
        string recipientName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        Dictionary<string, string>? customHeaders = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email is required.", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));

        if (string.IsNullOrWhiteSpace(htmlBody))
            throw new ArgumentException("HTML body is required.", nameof(htmlBody));

        try
        {
            _logger.LogInformation(
                "Sending Brevo raw email to {RecipientEmail} for tenant {TenantId}",
                recipientEmail, tenantId);

            var client = _httpClientFactory.CreateClient("Brevo");

            var payload = new
            {
                to = new[] { new { email = recipientEmail, name = recipientName } },
                sender = new { email = _options.SenderEmail, name = _options.SenderName },
                subject = subject,
                htmlContent = htmlBody,
                textContent = plainTextBody,
                headers = customHeaders
            };

            var response = await client.PostAsJsonAsync(
                "/smtp/email",
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Brevo raw email send failed for {RecipientEmail}. Status: {StatusCode}, Error: {Error}",
                    recipientEmail, response.StatusCode, errorContent);

                return new EmailSendResult
                {
                    Success = false,
                    ErrorCode = response.StatusCode.ToString(),
                    ErrorMessage = "Failed to send email through Brevo"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDoc = JsonSerializer.Deserialize<BrevEmailResponse>(responseContent);

            _logger.LogInformation(
                "Brevo raw email sent successfully to {RecipientEmail}. MessageId: {MessageId}",
                recipientEmail, jsonDoc?.MessageId);

            return new EmailSendResult
            {
                Success = true,
                ExternalMessageId = jsonDoc?.MessageId.ToString(),
                SentAtUtc = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception while sending Brevo raw email to {RecipientEmail}",
                recipientEmail);

            return new EmailSendResult
            {
                Success = false,
                ErrorCode = "EXCEPTION",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<BulkEmailSendResult> SendBulkTemplateEmailAsync(
        Guid tenantId,
        List<BulkEmailRecipient> recipients,
        string templateId,
        Dictionary<string, string>? commonVariables = null,
        CancellationToken cancellationToken = default)
    {
        if (recipients == null || !recipients.Any())
            throw new ArgumentException("Recipients list cannot be empty.", nameof(recipients));

        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Template ID is required.", nameof(templateId));

        var result = new BulkEmailSendResult();

        foreach (var recipient in recipients)
        {
            var variables = new Dictionary<string, string>(commonVariables ?? new());
            foreach (var (key, value) in recipient.Variables)
            {
                variables[key] = value;
            }

            var sendResult = await SendTemplateEmailAsync(
                tenantId,
                recipient.Email,
                recipient.Name,
                templateId,
                variables,
                cancellationToken: cancellationToken);

            if (sendResult.Success)
            {
                result.SuccessCount++;
                result.Results.Add(new BulkEmailItemResult
                {
                    RecipientEmail = recipient.Email,
                    Success = true,
                    ExternalMessageId = sendResult.ExternalMessageId
                });
            }
            else
            {
                result.FailureCount++;
                result.Results.Add(new BulkEmailItemResult
                {
                    RecipientEmail = recipient.Email,
                    Success = false,
                    ErrorCode = sendResult.ErrorCode,
                    ErrorMessage = sendResult.ErrorMessage
                });
            }
        }

        return result;
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
            _logger.LogError(ex, "Error verifying Brevo webhook signature");
            return false;
        }
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("Brevo");
            var response = await client.GetAsync("/account", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Brevo health check failed");
            return false;
        }
    }
}

/// <summary>
/// Brevo API response for email sending.
/// </summary>
internal class BrevEmailResponse
{
    [JsonPropertyName("messageId")]
    public long MessageId { get; set; }
}
