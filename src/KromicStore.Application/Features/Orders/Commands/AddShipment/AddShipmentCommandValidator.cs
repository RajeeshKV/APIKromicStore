using FluentValidation;

namespace KromicStore.Application.Features.Orders.Commands.AddShipment;

public class AddShipmentCommandValidator : AbstractValidator<AddShipmentCommand>
{
    public AddShipmentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");

        RuleFor(x => x.Carrier)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Carrier is required and must not exceed 100 characters");

        RuleFor(x => x.TrackingNumber)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Tracking number is required and must not exceed 100 characters");
    }
}
