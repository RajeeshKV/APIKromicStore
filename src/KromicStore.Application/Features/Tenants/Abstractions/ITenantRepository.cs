using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Abstractions;

/// <summary>
/// Repository abstraction for Tenant aggregate operations.
/// Implemented in Infrastructure layer; injected into Application handlers.
/// </summary>
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByCustomDomainAsync(string customDomain, CancellationToken cancellationToken = default);
    Task<bool> SubdomainExistsAsync(string subdomain, Guid? excludeTenantId = null, CancellationToken cancellationToken = default);
    Task<bool> CustomDomainExistsAsync(string customDomain, Guid? excludeTenantId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    void Update(Tenant tenant);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
