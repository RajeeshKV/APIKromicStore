using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.UpdateCartItem;

/// <summary>
/// Validator for UpdateCartItem command.
/// Validates cart ID, product ID, and new quantity.
/// </summary>
public sealed class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("CartId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative")
            .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000 items")
            .WithName("NewQuantity");

        // VariantId is optional, so no validation required
    }
}
