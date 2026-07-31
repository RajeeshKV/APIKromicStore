using KromicStore.Domain.Shipping.Entities;

namespace KromicStore.Application.Features.Shipping.Abstractions;

public interface IShippingZoneRepository
{
    Task<ShippingZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ShippingZone>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<ShippingZone>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ShippingZone?> GetByCountryAsync(string countryCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(ShippingZone zone);
    void Update(ShippingZone zone);
    void Delete(ShippingZone zone);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
