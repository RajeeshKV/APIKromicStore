using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.AddToWishlist;

/// <summary>
/// Validator for AddToWishlist command.
/// Validates wishlist ID and product ID.
/// </summary>
public sealed class AddToWishlistCommandValidator : AbstractValidator<AddToWishlistCommand>
{
    public AddToWishlistCommandValidator()
    {
        RuleFor(x => x.WishlistId)
            .NotEmpty().WithMessage("WishlistId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        // VariantId is optional
    }
}
