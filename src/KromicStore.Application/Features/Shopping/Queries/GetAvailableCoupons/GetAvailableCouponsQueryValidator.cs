using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Queries.GetAvailableCoupons;

/// <summary>
/// Validator for GetAvailableCoupons query.
/// Validates optional filters.
/// </summary>
public sealed class GetAvailableCouponsQueryValidator : AbstractValidator<GetAvailableCouponsQuery>
{
    public GetAvailableCouponsQueryValidator()
    {
        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("MinimumOrderAmount cannot be negative")
            .When(x => x.MinimumOrderAmount.HasValue);

        RuleFor(x => x.MaxResults)
            .GreaterThan(0).WithMessage("MaxResults must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("MaxResults cannot exceed 100")
            .When(x => x.MaxResults.HasValue);
    }
}
