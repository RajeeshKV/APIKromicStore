using FluentValidation;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator(IReservedSubdomainService reservedSubdomainService)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MinimumLength(2).WithMessage("Tenant name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Tenant name cannot exceed 100 characters.");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required.")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters.")
            .MaximumLength(63).WithMessage("Subdomain cannot exceed 63 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens.")
            .Must(s => !reservedSubdomainService.IsReserved(s)).WithMessage("This subdomain is reserved and cannot be used.");

        RuleFor(x => x.StoreName)
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.StoreName))
            .WithMessage("Store name cannot exceed 100 characters.");
    }
}
