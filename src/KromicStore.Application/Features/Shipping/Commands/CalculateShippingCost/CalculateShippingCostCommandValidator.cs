using FluentValidation;

namespace KromicStore.Application.Features.Shipping.Commands.CalculateShippingCost;

public sealed class CalculateShippingCostCommandValidator : AbstractValidator<CalculateShippingCostCommand>
{
    public CalculateShippingCostCommandValidator()
    {
        RuleFor(x => x.ShippingMethodId)
            .NotEmpty()
            .WithMessage("Shipping method is required");
        
        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Weight cannot be negative");
        
        RuleFor(x => x.OrderValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Order value cannot be negative");
    }
}
