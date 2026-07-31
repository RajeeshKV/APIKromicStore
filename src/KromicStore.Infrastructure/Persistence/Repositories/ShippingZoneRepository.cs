using KromicStore.Application.Features.Shipping.Abstractions;
using KromicStore.Domain.Shipping.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

public class ShippingZoneRepository : IShippingZoneRepository
{
    private readonly KromicStoreDbContext _dbContext;

    public ShippingZoneRepository(KromicStoreDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ShippingZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingZones.FirstOrDefaultAsync(z => z.Id == id, cancellationToken);
    }

    public async Task<List<ShippingZone>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingZones.ToListAsync(cancellationToken);
    }

    public async Task<List<ShippingZone>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingZones
            .Where(z => z.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<ShippingZone?> GetByCountryAsync(string countryCode, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingZones
            .FirstOrDefaultAsync(z => z.Countries.Contains(countryCode.ToUpperInvariant()), cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingZones.AnyAsync(z => z.Id == id, cancellationToken);
    }

    public void Add(ShippingZone zone)
    {
        _dbContext.ShippingZoneSet.Add(zone);
    }

    public void Update(ShippingZone zone)
    {
        _dbContext.ShippingZoneSet.Update(zone);
    }

    public void Delete(ShippingZone zone)
    {
        _dbContext.ShippingZoneSet.Remove(zone);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
