using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.SelectShippingMethod;

/// <summary>
/// Validator for SelectShippingMethod command.
/// Validates shipping method ID and cost.
/// </summary>
public sealed class SelectShippingMethodCommandValidator : AbstractValidator<SelectShippingMethodCommand>
{
    public SelectShippingMethodCommandValidator()
    {
        RuleFor(x => x.CheckoutSessionId)
            .NotEmpty().WithMessage("CheckoutSessionId is required");

        RuleFor(x => x.ShippingMethodId)
            .NotEmpty().WithMessage("ShippingMethodId is required")
            .MaximumLength(100).WithMessage("ShippingMethodId cannot exceed 100 characters");

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Shipping cost cannot be negative")
            .LessThanOrEqualTo(decimal.MaxValue / 1000).WithMessage("Shipping cost exceeds maximum allowed value");
    }
}
