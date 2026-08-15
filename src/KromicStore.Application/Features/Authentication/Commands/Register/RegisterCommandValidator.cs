using FluentValidation;

namespace KromicStore.Application.Features.Authentication.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required.")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters.")
            .MaximumLength(63).WithMessage("Subdomain cannot exceed 63 characters.")
            .Matches("^[a-z0-9][a-z0-9-]*[a-z0-9]$")
            .WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens. Cannot start or end with a hyphen.")
            .When(x => !string.IsNullOrWhiteSpace(x.Subdomain) && x.Subdomain.Length >= 2);

        RuleFor(x => x.StoreName)
            .MaximumLength(100).WithMessage("Store name cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.StoreName));
    }
}
