using FluentValidation;

namespace KromicStore.Application.Features.Shopping.Commands.UpdateBillingAddress;

/// <summary>
/// Validator for UpdateBillingAddress command.
/// Validates address fields.
/// </summary>
public sealed class UpdateBillingAddressCommandValidator : AbstractValidator<UpdateBillingAddressCommand>
{
    public UpdateBillingAddressCommandValidator()
    {
        RuleFor(x => x.CheckoutSessionId)
            .NotEmpty().WithMessage("CheckoutSessionId is required");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(255).WithMessage("Street cannot exceed 255 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required")
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("PostalCode is required")
            .MaximumLength(20).WithMessage("PostalCode cannot exceed 20 characters");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .Length(2).WithMessage("Country must be a valid ISO 3166-1 alpha-2 code (2 characters)");
    }
}
