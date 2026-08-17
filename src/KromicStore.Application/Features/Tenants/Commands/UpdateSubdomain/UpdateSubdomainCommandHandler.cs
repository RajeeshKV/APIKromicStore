using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Exceptions;
using KromicStore.Domain.Tenants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateSubdomain;

/// <summary>
/// Changes a tenant's platform subdomain.
///
/// Steps:
///   1. Validate new subdomain format (done by FluentValidation).
///   2. Check it isn't already taken by another tenant.
///   3. Load the tenant.
///   4. Remove the old primary platform domain and add the new one.
///   5. Update Tenant.Slug to match.
///   6. Persist.
/// </summary>
public sealed class UpdateSubdomainCommandHandler : IRequestHandler<UpdateSubdomainCommand, UpdateSubdomainResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<UpdateSubdomainCommandHandler> _logger;

    public UpdateSubdomainCommandHandler(
        ITenantRepository tenantRepository,
        ILogger<UpdateSubdomainCommandHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateSubdomainResponse> Handle(
        UpdateSubdomainCommand request,
        CancellationToken cancellationToken)
    {
        var newSlug = request.NewSubdomain.Trim().ToLowerInvariant();

        // 1. Check availability (exclude current tenant so it can "keep" its own slug on re-save)
        var taken = await _tenantRepository.SubdomainExistsAsync(
            newSlug, excludeTenantId: request.TenantId, cancellationToken: cancellationToken);

        if (taken)
            throw new ConflictException($"The subdomain '{newSlug}' is already taken. Please choose another.");

        // 2. Load tenant
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
            throw new NotFoundException($"Tenant {request.TenantId} not found.");

        // 3. Find the current primary platform domain and remove it, then add the new one.
        //    The Tenant domain model exposes RemoveDomain(domainId) and AddPlatformDomain(subdomain, isPrimary).
        var currentPrimaryDomain = tenant.Domains
            .FirstOrDefault(d => d.IsPrimary && d.Subdomain != null && d.CustomDomain == null);

        if (currentPrimaryDomain is not null)
            tenant.RemoveDomain(currentPrimaryDomain.Id);

        tenant.AddPlatformDomain(newSlug, isPrimary: true);

        // 4. Sync the Tenant slug so subdomain resolution stays consistent
        //    Use the existing domain-level method via reflection-free approach:
        //    Tenant.Slug is set only in Create() — we expose a RenameSlug domain method.
        tenant.UpdateSlug(newSlug);

        _tenantRepository.Update(tenant);
        await _tenantRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Subdomain updated for TenantId={TenantId}: new subdomain={Subdomain}",
            request.TenantId, newSlug);

        return new UpdateSubdomainResponse(
            TenantId:  tenant.Id,
            Subdomain: newSlug,
            StoreUrl:  $"https://{newSlug}.kromic.in");
    }
}
