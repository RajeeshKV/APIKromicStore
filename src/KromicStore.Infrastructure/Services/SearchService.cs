using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Services;

/// <summary>
/// Product search service using database LIKE queries.
/// In production, this should be replaced with Elasticsearch or similar.
/// </summary>
public sealed class SearchService
{
    private readonly IApplicationDbContext _dbContext;

    public SearchService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(
        string? searchText,
        Guid? categoryId = null,
        decimal? priceMin = null,
        decimal? priceMax = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Status == ProductStatus.Active);

        // Search text filter
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearch = searchText.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(normalizedSearch) ||
                p.Sku.ToLower().Contains(normalizedSearch) ||
                (p.Description != null && p.Description.ToLower().Contains(normalizedSearch)));
        }

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        // Price range filter
        if (priceMin.HasValue)
        {
            query = query.Where(p => p.Price >= priceMin.Value);
        }

        if (priceMax.HasValue)
        {
            query = query.Where(p => p.Price <= priceMax.Value);
        }

        // TODO: Add tag filtering when tag collection is loaded
        // This requires including the Tags navigation property

        return await query
            .OrderByDescending(p => p.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetSimilarProductsAsync(
        Guid productId,
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        var sourceProduct = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (sourceProduct is null)
            return [];

        var categoryId = sourceProduct.CategoryId;

        // Find products in the same category, excluding the source
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId &&
                       p.Id != productId &&
                       p.Status == ProductStatus.Active)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByTagAsync(
        string tag,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return [];

        // TODO: Implement tag filtering when tag relationship is loaded
        // For now, return empty as this requires proper tag relationship querying
        return await Task.FromResult<IEnumerable<Product>>([]);
    }
}
