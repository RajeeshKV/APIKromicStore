using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Theme repository for tenant store customization and branding.
/// Supports theme creation, management, publishing, and tenant assignment.
/// </summary>
public sealed class ThemeRepository : IThemeRepository
{
    private readonly IApplicationDbContext _dbContext;

    public ThemeRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Theme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Themes
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Theme?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug, nameof(slug));

        return await _dbContext.Themes
            .FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), cancellationToken);
    }

    public async Task<List<Theme>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Themes
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Theme>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Themes
            .Where(t => t.IsPublished)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Theme> Themes, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 20,
        ThemeStatus? statusFilter = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Themes.AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(t => t.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLowerInvariant();
            query = query.Where(t => t.Name.ToLower().Contains(lowerSearch) ||
                                     t.Description!.ToLower().Contains(lowerSearch));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var themes = await query
            .OrderByDescending(t => t.CreatedOnUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (themes, totalCount);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug, nameof(slug));

        var normalizedSlug = slug.ToLowerInvariant();
        var query = _dbContext.Themes
            .Where(t => t.Slug == normalizedSlug);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<List<Theme>> GetMostUsedAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Themes
            .Where(t => t.IsPublished)
            .OrderByDescending(t => t.TimesUsed)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public void Add(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme, nameof(theme));
        _dbContext.AddEntity(theme);
    }

    public void Update(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme, nameof(theme));
        // Update is handled by EF Core tracking
    }

    public void Remove(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme, nameof(theme));
        theme.Archive();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
