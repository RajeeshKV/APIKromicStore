using FluentValidation;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateStoreSettings;

public sealed class UpdateStoreSettingsCommandValidator : AbstractValidator<UpdateStoreSettingsCommand>
{
    public UpdateStoreSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEqual(Guid.Empty).WithMessage("TenantId is required.");

        RuleFor(x => x.StoreName)
            .MaximumLength(255).WithMessage("StoreName cannot exceed 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Email must be a valid email address.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s\-\(\)]{7,}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber)).WithMessage("PhoneNumber must be a valid phone number.");

        RuleFor(x => x.CurrencyCode)
            .Length(3).When(x => !string.IsNullOrEmpty(x.CurrencyCode)).WithMessage("CurrencyCode must be 3 characters (ISO 4217).");
    }
}
