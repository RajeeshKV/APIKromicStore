using FluentValidation;

namespace KromicStore.Application.StoreOperations.Commands.CreateFulfillment;

public sealed class CreateFulfillmentValidator : AbstractValidator<CreateFulfillmentCommand>
{
    public CreateFulfillmentValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");
        
        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .MaximumLength(300).WithMessage("Shipping address cannot exceed 300 characters");
        
        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("Shipping cost cannot be negative");
    }
}
