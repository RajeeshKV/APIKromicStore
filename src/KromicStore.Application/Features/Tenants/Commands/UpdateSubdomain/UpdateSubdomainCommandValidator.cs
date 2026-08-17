using FluentValidation;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateSubdomain;

public sealed class UpdateSubdomainCommandValidator : AbstractValidator<UpdateSubdomainCommand>
{
    public UpdateSubdomainCommandValidator(IReservedSubdomainService reservedSubdomainService)
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.NewSubdomain)
            .NotEmpty().WithMessage("Subdomain is required.")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters.")
            .MaximumLength(63).WithMessage("Subdomain cannot exceed 63 characters.")
            .Matches("^[a-z0-9][a-z0-9-]*[a-z0-9]$")
            .WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens. Cannot start or end with a hyphen.")
            .When(x => !string.IsNullOrWhiteSpace(x.NewSubdomain) && x.NewSubdomain.Length >= 2)
            .Must(s => !reservedSubdomainService.IsReserved(s))
            .WithMessage("This subdomain is reserved by the platform.");
    }
}
