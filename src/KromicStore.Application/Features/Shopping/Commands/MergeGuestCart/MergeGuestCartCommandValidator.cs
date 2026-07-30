using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.MergeGuestCart;

/// <summary>
/// Validator for MergeGuestCart command.
/// Validates customer ID and anonymous session ID.
/// </summary>
public sealed class MergeGuestCartCommandValidator : AbstractValidator<MergeGuestCartCommand>
{
    public MergeGuestCartCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");

        RuleFor(x => x.AnonymousSessionId)
            .NotEmpty().WithMessage("AnonymousSessionId is required")
            .MaximumLength(255).WithMessage("AnonymousSessionId cannot exceed 255 characters");
    }
}
