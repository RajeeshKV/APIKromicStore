using KromicStore.Application.Common.Repositories;
using KromicStore.Domain.CustomerPortal.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of customer address repository.
/// </summary>
public sealed class CustomerAddressRepository : ICustomerAddressRepository
{
    private readonly KromicStoreDbContext _context;
    
    public CustomerAddressRepository(KromicStoreDbContext context)
    {
        _context = context;
    }
    
    public async Task<CustomerAddress?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId, cancellationToken);
    }
    
    public async Task<IEnumerable<CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .Where(x => x.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<CustomerAddress?> GetDefaultShippingAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.IsDefaultShipping, cancellationToken);
    }
    
    public async Task<CustomerAddress?> GetDefaultBillingAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.IsDefaultBilling, cancellationToken);
    }
    
    public async Task AddAsync(CustomerAddress address, CancellationToken cancellationToken = default)
    {
        await _context.CustomerAddressSet.AddAsync(address, cancellationToken);
    }
    
    public Task UpdateAsync(CustomerAddress address, CancellationToken cancellationToken = default)
    {
        _context.CustomerAddressSet.Update(address);
        return Task.CompletedTask;
    }
    
    public Task DeleteAsync(CustomerAddress address, CancellationToken cancellationToken = default)
    {
        _context.CustomerAddressSet.Remove(address);
        return Task.CompletedTask;
    }
}
