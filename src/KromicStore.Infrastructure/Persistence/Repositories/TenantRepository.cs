using KromicStore.Domain.Tenants;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly KromicStoreDbContext _context;

    public TenantRepository(KromicStoreDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSubdomain(subdomain);
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Domains.Any(d => d.Subdomain == normalized), cancellationToken);
    }

    public async Task<Tenant?> GetByCustomDomainAsync(string customDomain, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeHost(customDomain);
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Domains.Any(d => d.CustomDomain == normalized && d.IsVerified), cancellationToken);
    }

    public async Task<bool> SubdomainExistsAsync(string subdomain, Guid? excludeTenantId = null, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSubdomain(subdomain);
        var query = _context.Tenants
            .Where(t => t.Domains.Any(d => d.Subdomain == normalized));

        if (excludeTenantId.HasValue)
            query = query.Where(t => t.Id != excludeTenantId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> CustomDomainExistsAsync(string customDomain, Guid? excludeTenantId = null, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeHost(customDomain);
        var query = _context.Tenants
            .Where(t => t.Domains.Any(d => d.CustomDomain == normalized));

        if (excludeTenantId.HasValue)
            query = query.Where(t => t.Id != excludeTenantId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (tenant == null) throw new ArgumentNullException(nameof(tenant));
        _context.Add(tenant);
        await Task.CompletedTask;
    }

    public void Update(Tenant tenant)
    {
        if (tenant == null) throw new ArgumentNullException(nameof(tenant));
        _context.Update(tenant);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    private static string NormalizeSubdomain(string subdomain)
        => subdomain.Trim().ToLowerInvariant();

    private static string NormalizeHost(string host)
        => host.Trim().TrimEnd('.').ToLowerInvariant();
}
