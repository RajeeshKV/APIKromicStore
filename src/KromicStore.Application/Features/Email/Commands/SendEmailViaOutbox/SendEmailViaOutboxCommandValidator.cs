using FluentValidation;

namespace KromicStore.Application.Features.Email.Commands.SendEmailViaOutbox;

/// <summary>
/// Validator for SendEmailViaOutboxCommand.
/// </summary>
public class SendEmailViaOutboxCommandValidator : AbstractValidator<SendEmailViaOutboxCommand>
{
    public SendEmailViaOutboxCommandValidator()
    {
        RuleFor(x => x.RecipientEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Recipient email must be a valid email address");

        RuleFor(x => x.RecipientName)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Recipient name is required and must not exceed 255 characters");

        // Either template-based or raw email must be provided
        RuleFor(x => x)
            .Custom((request, context) =>
            {
                bool hasTemplate = !string.IsNullOrWhiteSpace(request.TemplateType) && request.TemplateId > 0;
                bool hasRawEmail = !string.IsNullOrWhiteSpace(request.Subject) && 
                                  !string.IsNullOrWhiteSpace(request.HtmlBody);

                if (!hasTemplate && !hasRawEmail)
                {
                    context.AddFailure(
                        "Either template (TemplateType + TemplateId) or raw email (Subject + HtmlBody) must be provided");
                }

                if (hasTemplate && hasRawEmail)
                {
                    context.AddFailure(
                        "Cannot specify both template and raw email");
                }
            });

        // Template validation
        RuleFor(x => x.TemplateType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.TemplateType))
            .WithMessage("Template type must not exceed 100 characters");

        RuleFor(x => x.TemplateId)
            .GreaterThan(0)
            .When(x => !string.IsNullOrWhiteSpace(x.TemplateType))
            .WithMessage("Template ID must be greater than 0 when template type is specified");

        // Raw email validation
        RuleFor(x => x.Subject)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Subject))
            .WithMessage("Subject must not exceed 500 characters");

        RuleFor(x => x.HtmlBody)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.Subject))
            .WithMessage("HTML body is required when subject is provided");

        RuleFor(x => x.PlainTextBody)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.PlainTextBody))
            .WithMessage("Plain text body must not exceed 4000 characters");
    }
}
