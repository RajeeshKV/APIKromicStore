using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Queries.GetShippingMethods;

/// <summary>
/// Validator for GetShippingMethods query.
/// Validates optional country code.
/// </summary>
public sealed class GetShippingMethodsQueryValidator : AbstractValidator<GetShippingMethodsQuery>
{
    public GetShippingMethodsQueryValidator()
    {
        RuleFor(x => x.CountryCode)
            .Length(2).WithMessage("CountryCode must be a valid ISO 3166-1 alpha-2 code (2 characters)")
            .When(x => !string.IsNullOrWhiteSpace(x.CountryCode));
    }
}
