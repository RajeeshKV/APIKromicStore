using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.PlaceOrder;

/// <summary>
/// Validator for PlaceOrder command.
/// Validates checkout session ID and payment transaction ID.
/// </summary>
public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.CheckoutSessionId)
            .NotEmpty().WithMessage("CheckoutSessionId is required");

        RuleFor(x => x.PaymentTransactionId)
            .NotEmpty().WithMessage("PaymentTransactionId is required")
            .MaximumLength(255).WithMessage("PaymentTransactionId cannot exceed 255 characters");
    }
}
