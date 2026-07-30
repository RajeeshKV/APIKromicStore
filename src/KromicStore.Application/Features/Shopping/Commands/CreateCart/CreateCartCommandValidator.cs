using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.CreateCart;

/// <summary>
/// Validator for CreateCart command.
/// Validates that either CustomerId or AnonymousSessionId is provided, and currency is valid.
/// </summary>
public sealed class CreateCartCommandValidator : AbstractValidator<CreateCartCommand>
{
    public CreateCartCommandValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a valid ISO 4217 code (3 characters)");

        RuleFor(x => x)
            .Must(cmd =>
                (cmd.CustomerId.HasValue && cmd.CustomerId != Guid.Empty) ||
                !string.IsNullOrWhiteSpace(cmd.AnonymousSessionId))
            .WithMessage("Either CustomerId or AnonymousSessionId must be provided");

        // If CustomerId is provided, it must be a valid GUID
        RuleFor(x => x.CustomerId)
            .Must(id => !id.HasValue || id != Guid.Empty)
            .WithMessage("CustomerId, if provided, must not be empty");

        // If AnonymousSessionId is provided, it must have a reasonable length
        RuleFor(x => x.AnonymousSessionId)
            .MaximumLength(255).WithMessage("AnonymousSessionId cannot exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AnonymousSessionId));
    }
}
