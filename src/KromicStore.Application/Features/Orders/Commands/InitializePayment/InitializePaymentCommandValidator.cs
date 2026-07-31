using FluentValidation;

namespace KromicStore.Application.Features.Orders.Commands.InitializePayment;

public sealed class InitializePaymentCommandValidator : AbstractValidator<InitializePaymentCommand>
{
    public InitializePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a valid ISO 4217 code (3 characters)");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Payment method is required and must not exceed 100 characters");
    }
}
