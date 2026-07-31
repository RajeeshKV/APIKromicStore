using FluentValidation;

namespace KromicStore.Application.CustomerPortal.Commands.UpdateAddress;

public sealed class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Address ID is required");
        
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
        
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
        
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
