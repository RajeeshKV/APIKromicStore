using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Queries.GetCartByCustomer;

/// <summary>
/// Validator for GetCartByCustomer query.
/// Validates customer ID.
/// </summary>
public sealed class GetCartByCustomerQueryValidator : AbstractValidator<GetCartByCustomerQuery>
{
    public GetCartByCustomerQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");
    }
}
