using KromicStore.Application.Common.Repositories;
using KromicStore.Domain.StoreOperations.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of return request repository.
/// </summary>
public sealed class ReturnRequestRepository : IReturnRequestRepository
{
    private readonly KromicStoreDbContext _context;
    
    public ReturnRequestRepository(KromicStoreDbContext context)
    {
        _context = context;
    }
    
    public async Task<ReturnRequest?> GetByIdAsync(Guid returnRequestId, CancellationToken cancellationToken = default)
    {
        return await _context.ReturnRequestSet
            .FirstOrDefaultAsync(x => x.Id == returnRequestId, cancellationToken);
    }
    
    public async Task<IEnumerable<ReturnRequest>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.ReturnRequestSet
            .Where(x => x.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<ReturnRequest>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.ReturnRequestSet
            .Where(x => x.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<ReturnRequest>> GetByStatusAsync(string status, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var statusEnum = Enum.Parse<ReturnStatus>(status);
        return await _context.ReturnRequestSet
            .Where(x => x.Status == statusEnum)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddAsync(ReturnRequest returnRequest, CancellationToken cancellationToken = default)
    {
        await _context.ReturnRequestSet.AddAsync(returnRequest, cancellationToken);
    }
    
    public Task UpdateAsync(ReturnRequest returnRequest, CancellationToken cancellationToken = default)
    {
        _context.ReturnRequestSet.Update(returnRequest);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(ReturnRequest returnRequest, CancellationToken cancellationToken = default)
    {
        _context.ReturnRequestSet.Remove(returnRequest);
        return Task.CompletedTask;
    }
}
