using KromicStore.Application.Common.Abstractions;
using MediatR;

namespace KromicStore.Application.Features.Email.Commands.SendEmailViaOutbox;

/// <summary>
/// Command to send email via outbox pattern (reliable, with retry).
/// </summary>
public record SendEmailViaOutboxCommand : IRequest<SendEmailViaOutboxResult>
{
    /// <summary>
    /// Recipient email address.
    /// </summary>
    public required string RecipientEmail { get; init; }

    /// <summary>
    /// Recipient name.
    /// </summary>
    public required string RecipientName { get; init; }

    /// <summary>
    /// Email template type (e.g., "VerifyEmail", "OrderConfirmation").
    /// </summary>
    public string? TemplateType { get; init; }

    /// <summary>
    /// Email template ID (if using templates).
    /// </summary>
    public long TemplateId { get; init; }

    /// <summary>
    /// Template variables for substitution.
    /// </summary>
    public Dictionary<string, string>? Variables { get; init; }

    /// <summary>
    /// Email subject (if sending raw email).
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Email HTML body (if sending raw email).
    /// </summary>
    public string? HtmlBody { get; init; }

    /// <summary>
    /// Email plain text body (optional).
    /// </summary>
    public string? PlainTextBody { get; init; }

    /// <summary>
    /// Custom headers.
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; init; }
}

/// <summary>
/// Result of sending email via outbox.
/// </summary>
public record SendEmailViaOutboxResult
{
    public bool Success { get; init; }
    public Guid? OutboxId { get; init; }
    public string? ErrorMessage { get; init; }
}
