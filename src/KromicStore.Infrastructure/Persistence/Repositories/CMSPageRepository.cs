using KromicStore.Application.Features.CMS.Abstractions;
using KromicStore.Domain.CMS.Entities;
using KromicStore.Application.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for CMS page persistence.
/// </summary>
public sealed class CMSPageRepository : ICMSPageRepository
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CMSPageRepository> _logger;

    public CMSPageRepository(IApplicationDbContext dbContext, ILogger<CMSPageRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a CMS page by ID.
    /// </summary>
    public async Task<CMSPage?> GetByIdAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CMSPages
            .FirstOrDefaultAsync(p => p.Id == pageId && !p.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Gets a published CMS page by slug.
    /// </summary>
    public async Task<CMSPage?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.ToLower().Trim();
        return await _dbContext.CMSPages
            .FirstOrDefaultAsync(
                p => p.Slug == normalizedSlug
                    && p.Status == CMSPageStatus.Published
                    && !p.IsDeleted,
                cancellationToken);
    }

    /// <summary>
    /// Gets all published pages for a tenant.
    /// </summary>
    public async Task<IEnumerable<CMSPage>> GetPublishedPagesAsync(
        Guid tenantId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CMSPages
            .Where(p => p.TenantId == tenantId
                && p.Status == CMSPageStatus.Published
                && !p.IsDeleted)
            .OrderByDescending(p => p.PublishedDateUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all pages (published, draft, scheduled) for admin.
    /// </summary>
    public async Task<IEnumerable<CMSPage>> GetAllPagesAsync(
        Guid tenantId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.CMSPages
            .Where(p => p.TenantId == tenantId && !p.IsDeleted)
            .OrderByDescending(p => p.ModifiedOnUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new CMS page.
    /// </summary>
    public void Add(CMSPage page)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        _dbContext.AddEntity(page);
    }

    /// <summary>
    /// Updates an existing CMS page.
    /// </summary>
    public void Update(CMSPage page)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        // Entity tracking handled by EF Core
    }

    /// <summary>
    /// Deletes a CMS page (soft delete).
    /// </summary>
    public async Task<bool> DeleteAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        var page = await GetByIdAsync(pageId, cancellationToken);
        if (page == null)
            return false;

        page.SoftDelete();
        return true;
    }

    /// <summary>
    /// Checks if a slug exists for a tenant (excluding a specific page).
    /// </summary>
    public async Task<bool> SlugExistsAsync(
        Guid tenantId,
        string slug,
        Guid? excludePageId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.ToLower().Trim();
        var query = _dbContext.CMSPages
            .Where(p => p.TenantId == tenantId
                && p.Slug == normalizedSlug
                && !p.IsDeleted);

        if (excludePageId.HasValue)
        {
            query = query.Where(p => p.Id != excludePageId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
