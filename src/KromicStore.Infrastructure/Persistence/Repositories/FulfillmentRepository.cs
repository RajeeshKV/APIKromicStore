using KromicStore.Application.Common.Repositories;
using KromicStore.Domain.StoreOperations.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of fulfillment repository.
/// </summary>
public sealed class FulfillmentRepository : IFulfillmentRepository
{
    private readonly KromicStoreDbContext _context;
    
    public FulfillmentRepository(KromicStoreDbContext context)
    {
        _context = context;
    }
    
    public async Task<Fulfillment?> GetByIdAsync(Guid fulfillmentId, CancellationToken cancellationToken = default)
    {
        return await _context.FulfillmentSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == fulfillmentId, cancellationToken);
    }
    
    public async Task<Fulfillment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.FulfillmentSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
    }
    
    public async Task<IEnumerable<Fulfillment>> GetByStatusAsync(string status, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var statusEnum = Enum.Parse<FulfillmentStatus>(status);
        return await _context.FulfillmentSet
            .Where(x => x.Status == statusEnum)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Items)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Fulfillment>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FulfillmentSet
            .Where(x => x.Status == FulfillmentStatus.Pending || x.Status == FulfillmentStatus.Processing)
            .Include(x => x.Items)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddAsync(Fulfillment fulfillment, CancellationToken cancellationToken = default)
    {
        await _context.FulfillmentSet.AddAsync(fulfillment, cancellationToken);
    }
    
    public Task UpdateAsync(Fulfillment fulfillment, CancellationToken cancellationToken = default)
    {
        _context.FulfillmentSet.Update(fulfillment);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(Fulfillment fulfillment, CancellationToken cancellationToken = default)
    {
        _context.FulfillmentSet.Remove(fulfillment);
        return Task.CompletedTask;
    }
}
