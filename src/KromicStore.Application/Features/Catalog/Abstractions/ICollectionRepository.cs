using KromicStore.Domain.Catalog.Entities;

namespace KromicStore.Application.Features.Catalog.Abstractions;

public interface ICollectionRepository
{
    Task<ProductCollection?> GetByIdAsync(Guid collectionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductCollection>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    void Add(ProductCollection collection);
    void Update(ProductCollection collection);
    void Remove(ProductCollection collection);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
