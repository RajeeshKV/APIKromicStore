using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Queries.GetWishlistByCustomer;

/// <summary>
/// Validator for GetWishlistByCustomer query.
/// Validates customer ID.
/// </summary>
public sealed class GetWishlistByCustomerQueryValidator : AbstractValidator<GetWishlistByCustomerQuery>
{
    public GetWishlistByCustomerQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");
    }
}
