using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

public sealed class ProductCollectionRepository : ICollectionRepository
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ProductCollectionRepository(IApplicationDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<ProductCollection?> GetByIdAsync(Guid collectionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductCollections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == collectionId, cancellationToken);
    }

    public async Task<IEnumerable<ProductCollection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProductCollections
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = _dbContext.ProductCollections
            .AsNoTracking()
            .Where(c => c.Name == name);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public void Add(ProductCollection collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        _dbContext.AddEntity(collection);
    }

    public void Update(ProductCollection collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        _dbContext.AddEntity(collection);
    }

    public void Remove(ProductCollection collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        _dbContext.AddEntity(collection);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
