using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Queries.GetCart;

/// <summary>
/// Validator for GetCart query.
/// Validates cart ID.
/// </summary>
public sealed class GetCartQueryValidator : AbstractValidator<GetCartQuery>
{
    public GetCartQueryValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("CartId is required");
    }
}
