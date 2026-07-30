using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveCoupon;

/// <summary>
/// Validator for RemoveCoupon command.
/// Validates checkout session ID.
/// </summary>
public sealed class RemoveCouponCommandValidator : AbstractValidator<RemoveCouponCommand>
{
    public RemoveCouponCommandValidator()
    {
        RuleFor(x => x.CheckoutSessionId)
            .NotEmpty().WithMessage("CheckoutSessionId is required");
    }
}
