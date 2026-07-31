using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ProductRepository(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Status != ProductStatus.Draft)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.Status != ProductStatus.Draft)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetFeaturedAsync(int take = 20, CancellationToken cancellationToken = default)
    {
        if (take <= 0 || take > 100)
            take = 20;

        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsFeatured && p.Status == ProductStatus.Active)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return [];

        var normalizedSearch = searchText.Trim().ToLowerInvariant();

        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => (p.Name.ToLower().Contains(normalizedSearch) ||
                         p.Sku.ToLower().Contains(normalizedSearch) ||
                         p.Description.ToLower().Contains(normalizedSearch)) &&
                        p.Status != ProductStatus.Draft)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        var query = _dbContext.Products.AsNoTracking().Where(p => p.Sku == sku);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        var query = _dbContext.Products.AsNoTracking().Where(p => p.Slug == slug);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public void Add(Product product)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        _dbContext.AddEntity(product);
    }

    public void Update(Product product)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        _dbContext.AddEntity(product);
    }

    public void Remove(Product product)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));

        _dbContext.AddEntity(product);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return [];

        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.Status != ProductStatus.Draft)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return 0;

        return await _dbContext.Products
            .CountAsync(p => p.TenantId == tenantId && p.Status != ProductStatus.Draft, cancellationToken);
    }

    public async Task<int> GetLowStockCountByTenantIdAsync(Guid tenantId, int threshold = 10, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return 0;

        // TODO: Integrate with inventory/stock table when available
        // For now, return 0 as we need to query the inventory/stock entity
        // This would be: Count of products with stock quantity < threshold
        await Task.CompletedTask;
        return 0;
    }
}
