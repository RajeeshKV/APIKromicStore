using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Queries.GetWishlist;

/// <summary>
/// Validator for GetWishlist query.
/// Validates wishlist ID.
/// </summary>
public sealed class GetWishlistQueryValidator : AbstractValidator<GetWishlistQuery>
{
    public GetWishlistQueryValidator()
    {
        RuleFor(x => x.WishlistId)
            .NotEmpty().WithMessage("WishlistId is required");
    }
}
