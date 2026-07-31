using KromicStore.Application.Common.Repositories;
using KromicStore.Domain.StoreOperations.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of inventory adjustment repository.
/// </summary>
public sealed class InventoryAdjustmentRepository : IInventoryAdjustmentRepository
{
    private readonly KromicStoreDbContext _context;
    
    public InventoryAdjustmentRepository(KromicStoreDbContext context)
    {
        _context = context;
    }
    
    public async Task<InventoryAdjustment?> GetByIdAsync(Guid adjustmentId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryAdjustmentSet
            .FirstOrDefaultAsync(x => x.Id == adjustmentId, cancellationToken);
    }
    
    public async Task<IEnumerable<InventoryAdjustment>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.InventoryAdjustmentSet
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.RequestedOnUtc)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<InventoryAdjustment>> GetByStatusAsync(string status, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var statusEnum = Enum.Parse<AdjustmentStatus>(status);
        return await _context.InventoryAdjustmentSet
            .Where(x => x.Status == statusEnum)
            .OrderByDescending(x => x.RequestedOnUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<InventoryAdjustment>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InventoryAdjustmentSet
            .Where(x => x.Status == AdjustmentStatus.Pending)
            .OrderByDescending(x => x.RequestedOnUtc)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddAsync(InventoryAdjustment adjustment, CancellationToken cancellationToken = default)
    {
        await _context.InventoryAdjustmentSet.AddAsync(adjustment, cancellationToken);
    }
    
    public Task UpdateAsync(InventoryAdjustment adjustment, CancellationToken cancellationToken = default)
    {
        _context.InventoryAdjustmentSet.Update(adjustment);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(InventoryAdjustment adjustment, CancellationToken cancellationToken = default)
    {
        _context.InventoryAdjustmentSet.Remove(adjustment);
        return Task.CompletedTask;
    }
}
