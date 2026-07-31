using KromicStore.Domain.Catalog.Entities;

namespace KromicStore.Application.Features.Catalog.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetFeaturedAsync(int take = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchAsync(string searchText, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    void Add(Product product);
    void Update(Product product);
    void Remove(Product product);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all products for a specific tenant.
    /// </summary>
    Task<IEnumerable<Product>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of all products for a specific tenant.
    /// </summary>
    Task<int> GetCountByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of low stock products for a specific tenant.
    /// </summary>
    Task<int> GetLowStockCountByTenantIdAsync(Guid tenantId, int threshold = 10, CancellationToken cancellationToken = default);
}
