using KromicStore.Application.Features.Shipping.Abstractions;
using KromicStore.Domain.Shipping.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

public class ShippingMethodRepository : IShippingMethodRepository
{
    private readonly KromicStoreDbContext _dbContext;

    public ShippingMethodRepository(KromicStoreDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ShippingMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingMethods.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<List<ShippingMethod>> GetByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingMethods
            .Where(m => m.ShippingZoneId == zoneId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ShippingMethod>> GetActiveByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingMethods
            .Where(m => m.ShippingZoneId == zoneId && m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ShippingMethod>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingMethods.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShippingMethods.AnyAsync(m => m.Id == id, cancellationToken);
    }

    public void Add(ShippingMethod method)
    {
        _dbContext.ShippingMethodSet.Add(method);
    }

    public void Update(ShippingMethod method)
    {
        _dbContext.ShippingMethodSet.Update(method);
    }

    public void Delete(ShippingMethod method)
    {
        _dbContext.ShippingMethodSet.Remove(method);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
