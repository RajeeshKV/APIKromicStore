using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.InitializePayment;

/// <summary>
/// Validator for InitializePayment command.
/// Validates payment method.
/// </summary>
public sealed class InitializePaymentCommandValidator : AbstractValidator<InitializePaymentCommand>
{
    public InitializePaymentCommandValidator()
    {
        RuleFor(x => x.CheckoutSessionId)
            .NotEmpty().WithMessage("CheckoutSessionId is required");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("PaymentMethod is required")
            .Must(method => IsValidPaymentMethod(method)).WithMessage("Invalid payment method")
            .MaximumLength(50).WithMessage("PaymentMethod cannot exceed 50 characters");
    }

    private static bool IsValidPaymentMethod(string paymentMethod)
    {
        var validMethods = new[] { "CreditCard", "DebitCard", "PayPal", "BankTransfer", "ApplePay", "GooglePay" };
        return validMethods.Contains(paymentMethod);
    }
}
