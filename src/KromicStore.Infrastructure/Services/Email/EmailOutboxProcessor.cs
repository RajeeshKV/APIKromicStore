using KromicStore.Application.Features.Email.Abstractions;
using KromicStore.Domain.Email.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Services.Email;

/// <summary>
/// Processes emails from the outbox and sends them via Brevo.
/// Implements reliable delivery with retry logic.
/// </summary>
public class EmailOutboxProcessor
{
    private readonly IEmailOutboxRepository _outboxRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailOutboxProcessor> _logger;

    public EmailOutboxProcessor(
        IEmailOutboxRepository outboxRepository,
        IEmailService emailService,
        ILogger<EmailOutboxProcessor> logger)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Process pending emails from the outbox.
    /// </summary>
    public async Task<int> ProcessPendingAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting EmailOutbox processor. Batch size: {BatchSize}", batchSize);

        var pendingEmails = await _outboxRepository.GetPendingAsync(batchSize, cancellationToken);
        var emails = pendingEmails.ToList();

        if (!emails.Any())
        {
            _logger.LogDebug("No pending emails found in outbox");
            return 0;
        }

        int successCount = 0;
        int failureCount = 0;

        foreach (var email in emails)
        {
            try
            {
                await ProcessEmailAsync(email, cancellationToken);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing email outbox entry. OutboxId: {OutboxId}, Recipient: {Recipient}",
                    email.Id, email.RecipientEmail);
                failureCount++;
            }
        }

        _logger.LogInformation(
            "EmailOutbox processor completed. Processed: {ProcessedCount}, Success: {SuccessCount}, Failures: {FailureCount}",
            emails.Count, successCount, failureCount);

        return successCount;
    }

    /// <summary>
    /// Process emails ready for retry.
    /// </summary>
    public async Task<int> ProcessRetriesAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting EmailOutbox retry processor. Batch size: {BatchSize}", batchSize);

        var retryEmails = await _outboxRepository.GetReadyForRetryAsync(batchSize, cancellationToken);
        var emails = retryEmails.ToList();

        if (!emails.Any())
        {
            _logger.LogDebug("No emails ready for retry in outbox");
            return 0;
        }

        int successCount = 0;
        int failureCount = 0;

        foreach (var email in emails)
        {
            try
            {
                await ProcessEmailAsync(email, cancellationToken);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrying email outbox entry. OutboxId: {OutboxId}, Recipient: {Recipient}, Attempt: {Attempt}",
                    email.Id, email.RecipientEmail, email.AttemptCount + 1);
                failureCount++;
            }
        }

        _logger.LogInformation(
            "EmailOutbox retry processor completed. Processed: {ProcessedCount}, Success: {SuccessCount}, Failures: {FailureCount}",
            emails.Count, successCount, failureCount);

        return successCount;
    }

    /// <summary>
    /// Process a single email.
    /// </summary>
    private async Task ProcessEmailAsync(EmailOutbox email, CancellationToken cancellationToken)
    {
        email.MarkAsProcessing();
        await _outboxRepository.UpdateAsync(email, cancellationToken);

        _logger.LogInformation(
            "Processing email. OutboxId: {OutboxId}, Recipient: {Recipient}, Attempt: {Attempt}",
            email.Id, email.RecipientEmail, email.AttemptCount + 1);

        EmailSendResult result;

        // Send based on email type
        if (!string.IsNullOrWhiteSpace(email.TemplateType) && email.TemplateType != "RawEmail")
        {
            // Send template email
            result = await _emailService.SendTemplateEmailAsync(
                email.TenantId,
                email.RecipientEmail,
                email.RecipientName,
                email.TemplateId.ToString(),
                email.TemplateVariables ?? new Dictionary<string, string>(),
                email.CustomHeaders,
                cancellationToken);
        }
        else
        {
            // Send raw email
            result = await _emailService.SendRawEmailAsync(
                email.TenantId,
                email.RecipientEmail,
                email.RecipientName,
                email.Subject ?? "No Subject",
                email.HtmlBody ?? string.Empty,
                email.PlainTextBody,
                email.CustomHeaders,
                cancellationToken);
        }

        // Handle result
        if (result.Success)
        {
            email.MarkAsSent(result.ExternalMessageId ?? email.Id.ToString());
            _logger.LogInformation(
                "Email sent successfully. OutboxId: {OutboxId}, Recipient: {Recipient}, MessageId: {MessageId}",
                email.Id, email.RecipientEmail, result.ExternalMessageId);
        }
        else
        {
            email.MarkAsFailed(
                result.ErrorCode ?? "UNKNOWN_ERROR",
                result.ErrorMessage ?? "Unknown error",
                60 * email.AttemptCount); // Exponential backoff: 60s, 120s, 180s

            _logger.LogWarning(
                "Email send failed. OutboxId: {OutboxId}, Recipient: {Recipient}, Error: {Error}, " +
                "Attempt: {Attempt}/{MaxAttempts}, NextRetry: {NextRetry}",
                email.Id, email.RecipientEmail, result.ErrorMessage, 
                email.AttemptCount, email.MaxAttempts, email.NextRetryAtUtc);
        }

        await _outboxRepository.UpdateAsync(email, cancellationToken);
    }
}
