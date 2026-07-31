using FluentValidation;

namespace KromicStore.Application.CustomerPortal.Commands.DeleteAddress;

public sealed class DeleteAddressValidator : AbstractValidator<DeleteAddressCommand>
{
    public DeleteAddressValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Address ID is required");
        
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");
    }
}
