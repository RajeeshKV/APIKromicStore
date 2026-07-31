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

    /// <summary>
    /// Get all tenants with optional filtering and pagination.
    /// </summary>
    Task<(List<Tenant> Tenants, int TotalCount)> GetAllWithPaginationAsync(
        int skip = 0,
        int take = 20,
        TenantStatus? statusFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count tenants by status.
    /// </summary>
    Task<int> CountByStatusAsync(TenantStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of all tenants.
    /// </summary>
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tenants (no pagination).
    /// </summary>
    Task<List<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
}
