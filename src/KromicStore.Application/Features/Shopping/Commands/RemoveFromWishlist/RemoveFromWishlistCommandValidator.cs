using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveFromWishlist;

/// <summary>
/// Validator for RemoveFromWishlist command.
/// Validates wishlist ID and product ID.
/// </summary>
public sealed class RemoveFromWishlistCommandValidator : AbstractValidator<RemoveFromWishlistCommand>
{
    public RemoveFromWishlistCommandValidator()
    {
        RuleFor(x => x.WishlistId)
            .NotEmpty().WithMessage("WishlistId is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required");

        // VariantId is optional
    }
}
