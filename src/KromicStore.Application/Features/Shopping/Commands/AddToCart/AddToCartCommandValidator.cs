using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.AddToCart;

/// <summary>
/// Validator for AddToCart command.
/// Validates cart ID, product ID, price, and quantity.
/// </summary>
public sealed class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("CartId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative")
            .LessThanOrEqualTo(decimal.MaxValue / 1000).WithMessage("Unit price exceeds maximum allowed value");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000 items");

        // VariantId is optional, so no validation required
    }
}
