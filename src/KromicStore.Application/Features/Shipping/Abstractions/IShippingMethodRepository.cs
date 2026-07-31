using KromicStore.Domain.Shipping.Entities;

namespace KromicStore.Application.Features.Shipping.Abstractions;

public interface IShippingMethodRepository
{
    Task<ShippingMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ShippingMethod>> GetByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default);
    Task<List<ShippingMethod>> GetActiveByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default);
    Task<List<ShippingMethod>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(ShippingMethod method);
    void Update(ShippingMethod method);
    void Delete(ShippingMethod method);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
