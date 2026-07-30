using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveCartItem;

/// <summary>
/// Validator for RemoveCartItem command.
/// Validates cart ID and product ID.
/// </summary>
public sealed class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("CartId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        // VariantId is optional, so no validation required
    }
}
