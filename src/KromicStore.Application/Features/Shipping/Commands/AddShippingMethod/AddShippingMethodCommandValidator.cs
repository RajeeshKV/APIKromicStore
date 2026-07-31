using FluentValidation;

namespace KromicStore.Application.Features.Shipping.Commands.AddShippingMethod;

public sealed class AddShippingMethodCommandValidator : AbstractValidator<AddShippingMethodCommand>
{
    public AddShippingMethodCommandValidator()
    {
        RuleFor(x => x.ShippingZoneId)
            .NotEmpty()
            .WithMessage("Shipping zone is required");
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Method name is required")
            .MaximumLength(100)
            .WithMessage("Method name cannot exceed 100 characters");
        
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Method code is required")
            .MaximumLength(50)
            .WithMessage("Method code cannot exceed 50 characters");
        
        RuleFor(x => x.EstimatedDaysMin)
            .GreaterThan(0)
            .WithMessage("Minimum estimated days must be greater than 0");
        
        RuleFor(x => x.EstimatedDaysMax)
            .GreaterThanOrEqualTo(x => x.EstimatedDaysMin)
            .WithMessage("Maximum estimated days must be >= minimum");
    }
}
