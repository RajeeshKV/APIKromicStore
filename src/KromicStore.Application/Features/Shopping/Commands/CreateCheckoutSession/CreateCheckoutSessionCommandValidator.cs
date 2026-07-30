using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.CreateCheckoutSession;

/// <summary>
/// Validator for CreateCheckoutSession command.
/// Validates cart ID and customer ID.
/// </summary>
public sealed class CreateCheckoutSessionCommandValidator : AbstractValidator<CreateCheckoutSessionCommand>
{
    public CreateCheckoutSessionCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("CartId is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");
    }
}
