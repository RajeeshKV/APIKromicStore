using MediatR;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.AddCustomDomain;

public sealed class AddCustomDomainCommandHandler : IRequestHandler<AddCustomDomainCommand, AddCustomDomainResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<AddCustomDomainCommandHandler> _logger;

    public AddCustomDomainCommandHandler(ITenantRepository repository, ILogger<AddCustomDomainCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AddCustomDomainResponse> Handle(AddCustomDomainCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{request.TenantId}' not found.");

        // Check if custom domain already exists
        if (await _repository.CustomDomainExistsAsync(request.CustomDomain, excludeTenantId: request.TenantId, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException($"Custom domain '{request.CustomDomain}' is already in use.");
        }

        // Add custom domain
        var domain = TenantDomain.CreateCustomDomain(tenant.Id, request.CustomDomain, isPrimary: request.SetPrimary);
        tenant.AddDomain(domain);

        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Custom domain added to tenant: {TenantId} Domain={Domain}", request.TenantId, request.CustomDomain);

        return new AddCustomDomainResponse(
            TenantId: tenant.Id,
            CustomDomain: domain.CustomDomain ?? string.Empty,
            IsPrimary: domain.IsPrimary,
            IsVerified: domain.IsVerified);
    }
}
