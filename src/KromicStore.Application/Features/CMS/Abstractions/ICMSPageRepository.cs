using KromicStore.Domain.CMS.Entities;

namespace KromicStore.Application.Features.CMS.Abstractions;

/// <summary>
/// Repository for CMS page persistence.
/// </summary>
public interface ICMSPageRepository
{
    /// <summary>
    /// Gets a CMS page by ID.
    /// </summary>
    Task<CMSPage?> GetByIdAsync(Guid pageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a published CMS page by slug.
    /// </summary>
    Task<CMSPage?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all published pages for a tenant.
    /// </summary>
    Task<IEnumerable<CMSPage>> GetPublishedPagesAsync(
        Guid tenantId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pages (published, draft, scheduled) for admin.
    /// </summary>
    Task<IEnumerable<CMSPage>> GetAllPagesAsync(
        Guid tenantId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new CMS page.
    /// </summary>
    void Add(CMSPage page);

    /// <summary>
    /// Updates an existing CMS page.
    /// </summary>
    void Update(CMSPage page);

    /// <summary>
    /// Deletes a CMS page (soft delete).
    /// </summary>
    Task<bool> DeleteAsync(Guid pageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a slug exists for a tenant (excluding a specific page).
    /// </summary>
    Task<bool> SlugExistsAsync(Guid tenantId, string slug, Guid? excludePageId = null, CancellationToken cancellationToken = default);
}
