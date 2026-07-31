using FluentValidation;

namespace KromicStore.Application.Features.Promotions.Commands.ApplyCoupon;

public sealed class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponCommandValidator()
    {
        RuleFor(x => x.CouponCode)
            .NotEmpty()
            .WithMessage("Coupon code is required")
            .MaximumLength(100)
            .WithMessage("Coupon code cannot exceed 100 characters");
        
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");
        
        RuleFor(x => x.OrderAmount)
            .GreaterThan(0)
            .WithMessage("Order amount must be greater than 0");
    }
}
