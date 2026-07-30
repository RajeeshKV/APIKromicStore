using MediatR;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.RemoveCustomDomain;

public sealed class RemoveCustomDomainCommandHandler : IRequestHandler<RemoveCustomDomainCommand, Unit>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<RemoveCustomDomainCommandHandler> _logger;

    public RemoveCustomDomainCommandHandler(ITenantRepository repository, ILogger<RemoveCustomDomainCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(RemoveCustomDomainCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{request.TenantId}' not found.");

        // Find and remove the custom domain
        var domain = tenant.Domains.FirstOrDefault(d => 
            d.CustomDomain?.Equals(request.CustomDomain, StringComparison.OrdinalIgnoreCase) == true);

        if (domain is null)
        {
            throw new InvalidOperationException($"Custom domain '{request.CustomDomain}' not found for this tenant.");
        }

        // If this is the primary domain, reject removal
        if (domain.IsPrimary)
        {
            throw new InvalidOperationException("Cannot remove the primary domain. Set another domain as primary first.");
        }

        tenant.RemoveDomain(domain.Id);

        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Custom domain removed from tenant: {TenantId} Domain={Domain}", request.TenantId, request.CustomDomain);
        
        return Unit.Value;
    }
}
