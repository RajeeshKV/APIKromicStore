using KromicStore.Application.Common.Repositories;
using KromicStore.Domain.CustomerPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of customer profile repository.
/// </summary>
public sealed class CustomerProfileRepository : ICustomerProfileRepository
{
    private readonly KromicStoreDbContext _context;
    
    public CustomerProfileRepository(KromicStoreDbContext context)
    {
        _context = context;
    }
    
    public async Task<CustomerProfile?> GetByIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles
            .FirstOrDefaultAsync(x => x.Id == profileId, cancellationToken);
    }
    
    public async Task<CustomerProfile?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles
            .FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }
    
    public async Task<IEnumerable<CustomerProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CustomerProfiles
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        await _context.CustomerProfileSet.AddAsync(profile, cancellationToken);
    }
    
    public Task UpdateAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        _context.CustomerProfileSet.Update(profile);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(CustomerProfile profile, CancellationToken cancellationToken = default)
    {
        _context.CustomerProfileSet.Remove(profile);
        return Task.CompletedTask;
    }
}
