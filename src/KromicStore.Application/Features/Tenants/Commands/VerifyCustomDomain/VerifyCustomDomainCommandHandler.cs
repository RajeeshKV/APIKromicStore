using MediatR;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.VerifyCustomDomain;

public sealed class VerifyCustomDomainCommandHandler : IRequestHandler<VerifyCustomDomainCommand, VerifyCustomDomainResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ILogger<VerifyCustomDomainCommandHandler> _logger;

    public VerifyCustomDomainCommandHandler(ITenantRepository repository, ILogger<VerifyCustomDomainCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VerifyCustomDomainResponse> Handle(VerifyCustomDomainCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant '{request.TenantId}' not found.");

        // Find the custom domain
        var domain = tenant.Domains.FirstOrDefault(d =>
            d.CustomDomain?.Equals(request.CustomDomain, StringComparison.OrdinalIgnoreCase) == true);

        if (domain is null)
        {
            throw new InvalidOperationException($"Custom domain '{request.CustomDomain}' not found for this tenant.");
        }

        // Mark as verified
        domain.MarkVerified();

        _repository.Update(tenant);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Custom domain verified for tenant: {TenantId} Domain={Domain}", request.TenantId, request.CustomDomain);

        return new VerifyCustomDomainResponse(
            TenantId: tenant.Id,
            CustomDomain: domain.CustomDomain ?? string.Empty,
            IsVerified: domain.IsVerified);
    }
}
