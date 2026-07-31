namespace KromicStore.Application.Common.Repositories;

/// <summary>
/// Repository abstraction for customer addresses.
/// </summary>
public interface ICustomerAddressRepository
{
    Task<Domain.CustomerPortal.Entities.CustomerAddress?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.CustomerPortal.Entities.CustomerAddress>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Domain.CustomerPortal.Entities.CustomerAddress?> GetDefaultShippingAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Domain.CustomerPortal.Entities.CustomerAddress?> GetDefaultBillingAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.CustomerPortal.Entities.CustomerAddress address, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.CustomerPortal.Entities.CustomerAddress address, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.CustomerPortal.Entities.CustomerAddress address, CancellationToken cancellationToken = default);
}
