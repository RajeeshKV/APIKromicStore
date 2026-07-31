using FluentValidation;

namespace KromicStore.Application.CustomerPortal.Commands.AddAddress;

public sealed class AddAddressValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
        
        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Address label is required")
            .MaximumLength(50).WithMessage("Label cannot exceed 50 characters");
        
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street address is required")
            .MaximumLength(150).WithMessage("Street address cannot exceed 150 characters");
        
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");
        
        RuleFor(x => x.StateCode)
            .NotEmpty().WithMessage("State code is required")
            .MaximumLength(10).WithMessage("State code cannot exceed 10 characters");
        
        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters");
        
        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required")
            .Length(2).WithMessage("Country code must be exactly 2 characters (ISO 3166-1 alpha-2)");
        
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        
        RuleFor(x => x)
            .Must(x => x.IsShippingAddress || x.IsBillingAddress)
            .WithMessage("Address must be marked as either shipping or billing address");
    }
}
