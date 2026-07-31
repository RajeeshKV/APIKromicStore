using FluentValidation;

namespace KromicStore.Application.Features.Shipping.Commands.CreateShippingZone;

public sealed class CreateShippingZoneCommandValidator : AbstractValidator<CreateShippingZoneCommand>
{
    public CreateShippingZoneCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Zone name is required")
            .MaximumLength(200)
            .WithMessage("Zone name cannot exceed 200 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");
        
        RuleFor(x => x.Countries)
            .Must(countries => countries != null && countries.Count > 0)
            .WithMessage("At least one country is required")
            .ForEach(countryRule =>
            {
                countryRule
                    .NotEmpty()
                    .WithMessage("Country code cannot be empty")
                    .Length(2)
                    .WithMessage("Country code must be 2 characters (ISO 3166-1 alpha-2)")
                    .Matches("^[A-Z]{2}$")
                    .WithMessage("Country code must be uppercase letters only");
            });
    }
}
