using FluentValidation;

namespace KromicStore.Application.CustomerPortal.Commands.SetDefaultAddress;

public sealed class SetDefaultAddressValidator : AbstractValidator<SetDefaultAddressCommand>
{
    public SetDefaultAddressValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Address ID is required");
        
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
        
        RuleFor(x => x)
            .Must(x => x.IsShippingDefault || x.IsBillingDefault)
            .WithMessage("At least one default address type must be selected");
    }
}
