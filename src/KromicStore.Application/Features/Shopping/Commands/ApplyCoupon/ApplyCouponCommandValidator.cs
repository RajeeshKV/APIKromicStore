using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.ApplyCoupon;

/// <summary>
/// Validator for ApplyCoupon command.
/// Validates checkout session ID and coupon code.
/// </summary>
public sealed class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponCommandValidator()
    {
        RuleFor(x => x.CheckoutSessionId)
            .NotEmpty().WithMessage("CheckoutSessionId is required");

        RuleFor(x => x.CouponCode)
            .NotEmpty().WithMessage("CouponCode is required")
            .MaximumLength(50).WithMessage("CouponCode cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("CouponCode can only contain uppercase letters, numbers, and hyphens");
    }
}
