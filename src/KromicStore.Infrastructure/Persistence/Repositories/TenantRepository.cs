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

    public async Task<(List<Tenant> Tenants, int TotalCount)> GetAllWithPaginationAsync(
        int skip = 0,
        int take = 20,
        TenantStatus? statusFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        // Normalize take to reasonable limits
        if (take <= 0) take = 20;
        if (take > 100) take = 100;
        if (skip < 0) skip = 0;

        var query = _context.Tenants.AsQueryable();

        // Apply status filter if provided
        if (statusFilter.HasValue)
        {
            query = query.Where(t => t.Status == statusFilter.Value);
        }

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower().Trim();
            query = query.Where(t => 
                t.Name.ToLower().Contains(searchLower) || 
                t.StoreName.ToLower().Contains(searchLower) ||
                t.Slug.ToLower().Contains(searchLower));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and order
        var tenants = await query
            .OrderByDescending(t => t.CreatedOnUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (tenants, totalCount);
    }

    public async Task<int> CountByStatusAsync(TenantStatus status, CancellationToken cancellationToken = default)
        => await _context.Tenants.CountAsync(t => t.Status == status, cancellationToken);

    public async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
        => await _context.Tenants.CountAsync(cancellationToken);

    public async Task<List<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Tenants
            .OrderByDescending(t => t.CreatedOnUtc)
            .ToListAsync(cancellationToken);

    private static string NormalizeSubdomain(string subdomain)
        => subdomain.Trim().ToLowerInvariant();

    private static string NormalizeHost(string host)
        => host.Trim().TrimEnd('.').ToLowerInvariant();
}
