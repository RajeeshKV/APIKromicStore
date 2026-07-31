using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Email.Abstractions;
using KromicStore.Domain.Email.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Email.Commands.SendEmailViaOutbox;

/// <summary>
/// Handler for sending emails via outbox pattern.
/// </summary>
public class SendEmailViaOutboxCommandHandler : IRequestHandler<SendEmailViaOutboxCommand, SendEmailViaOutboxResult>
{
    private readonly IEmailOutboxRepository _outboxRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SendEmailViaOutboxCommandHandler> _logger;

    public SendEmailViaOutboxCommandHandler(
        IEmailOutboxRepository outboxRepository,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<SendEmailViaOutboxCommandHandler> logger)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SendEmailViaOutboxResult> Handle(SendEmailViaOutboxCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var tenantId = _tenantContext.TenantId;
        if (!tenantId.HasValue || tenantId == Guid.Empty)
        {
            var error = "Tenant context is not resolved";
            _logger.LogError(error);
            return new SendEmailViaOutboxResult { Success = false, ErrorMessage = error };
        }

        var userId = _currentUserService.UserId;

        EmailOutbox outbox;

        // Create outbox entry based on email type
        if (!string.IsNullOrWhiteSpace(request.TemplateType) && request.TemplateId > 0)
        {
            // Template-based email
            outbox = EmailOutbox.CreateTemplate(
                tenantId.Value,
                request.RecipientEmail,
                request.RecipientName,
                request.TemplateType,
                request.TemplateId,
                request.Variables,
                request.CustomHeaders,
                userId.ToString());

            _logger.LogInformation(
                "Created template email outbox entry. TenantId: {TenantId}, " +
                "Recipient: {Recipient}, Template: {Template}, OutboxId: {OutboxId}",
                tenantId.Value, request.RecipientEmail, request.TemplateType, outbox.Id);
        }
        else if (!string.IsNullOrWhiteSpace(request.Subject) && !string.IsNullOrWhiteSpace(request.HtmlBody))
        {
            // Raw email
            outbox = EmailOutbox.CreateRaw(
                tenantId.Value,
                request.RecipientEmail,
                request.RecipientName,
                request.Subject,
                request.HtmlBody,
                request.PlainTextBody,
                request.CustomHeaders,
                userId.ToString());

            _logger.LogInformation(
                "Created raw email outbox entry. TenantId: {TenantId}, " +
                "Recipient: {Recipient}, Subject: {Subject}, OutboxId: {OutboxId}",
                tenantId.Value, request.RecipientEmail, request.Subject, outbox.Id);
        }
        else
        {
            var error = "Either template ID or subject/body must be provided";
            _logger.LogError(error);
            return new SendEmailViaOutboxResult { Success = false, ErrorMessage = error };
        }

        try
        {
            // Save to outbox
            await _outboxRepository.AddAsync(outbox, cancellationToken);

            _logger.LogInformation(
                "Email queued in outbox. OutboxId: {OutboxId}, Recipient: {Recipient}",
                outbox.Id, request.RecipientEmail);

            return new SendEmailViaOutboxResult
            {
                Success = true,
                OutboxId = outbox.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to queue email in outbox. Recipient: {Recipient}",
                request.RecipientEmail);

            return new SendEmailViaOutboxResult
            {
                Success = false,
                ErrorMessage = $"Failed to queue email: {ex.Message}"
            };
        }
    }
}
