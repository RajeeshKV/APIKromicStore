using FluentValidation;

namespace KromicStore.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x)
            .Must(x => x.CustomerId != Guid.Empty || x.TenantId != Guid.Empty)
            .WithMessage("Either CustomerId or TenantId must be provided");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters");
    }
}
