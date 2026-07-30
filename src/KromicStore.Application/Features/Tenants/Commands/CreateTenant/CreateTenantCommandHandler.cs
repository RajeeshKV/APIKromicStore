using MediatR;
using KromicStore.Domain.Tenants;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.CreateTenant;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(ITenantRepository repository, ILogger<CreateTenantCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateTenantResponse> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Check if subdomain already exists
        if (await _repository.SubdomainExistsAsync(request.Subdomain, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException($"Subdomain '{request.Subdomain}' is already in use.");
        }

        // Create tenant aggregate
        var tenant = Tenant.Create(
            name: request.Name,
            slug: request.Subdomain,
            storeName: request.StoreName);

        // Add primary subdomain
        tenant.AddPlatformDomain(request.Subdomain, isPrimary: true);

        // Assign owner if provided
        if (request.OwnerUserId.HasValue)
        {
            tenant.AssignOwner(request.OwnerUserId.Value);
        }

        // Persist
        await _repository.AddAsync(tenant, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tenant created: {TenantId} ({Subdomain})", tenant.Id, request.Subdomain);

        return new CreateTenantResponse(
            TenantId: tenant.Id,
            Name: tenant.Name,
            Subdomain: request.Subdomain,
            StoreName: tenant.StoreName);
    }
}
