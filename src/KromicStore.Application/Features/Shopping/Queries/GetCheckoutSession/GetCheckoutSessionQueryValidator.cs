using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Queries.GetCheckoutSession;

/// <summary>
/// Validator for GetCheckoutSession query.
/// Validates checkout session ID.
/// </summary>
public sealed class GetCheckoutSessionQueryValidator : AbstractValidator<GetCheckoutSessionQuery>
{
    public GetCheckoutSessionQueryValidator()
    {
        RuleFor(x => x.CheckoutSessionId)
            .NotEmpty().WithMessage("CheckoutSessionId is required");
    }
}
