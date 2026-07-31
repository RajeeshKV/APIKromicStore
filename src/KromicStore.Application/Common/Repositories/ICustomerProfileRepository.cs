namespace KromicStore.Application.Common.Repositories;

/// <summary>
/// Repository abstraction for customer profiles.
/// </summary>
public interface ICustomerProfileRepository
{
    Task<Domain.CustomerPortal.Entities.CustomerProfile?> GetByIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<Domain.CustomerPortal.Entities.CustomerProfile?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.CustomerPortal.Entities.CustomerProfile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.CustomerPortal.Entities.CustomerProfile profile, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.CustomerPortal.Entities.CustomerProfile profile, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.CustomerPortal.Entities.CustomerProfile profile, CancellationToken cancellationToken = default);
}
