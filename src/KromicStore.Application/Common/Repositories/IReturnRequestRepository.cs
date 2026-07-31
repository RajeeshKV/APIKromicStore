namespace KromicStore.Application.Common.Repositories;

/// <summary>
/// Repository abstraction for return requests.
/// </summary>
public interface IReturnRequestRepository
{
    Task<Domain.StoreOperations.Entities.ReturnRequest?> GetByIdAsync(Guid returnRequestId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.StoreOperations.Entities.ReturnRequest>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.StoreOperations.Entities.ReturnRequest>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.StoreOperations.Entities.ReturnRequest>> GetByStatusAsync(string status, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.StoreOperations.Entities.ReturnRequest returnRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.StoreOperations.Entities.ReturnRequest returnRequest, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.StoreOperations.Entities.ReturnRequest returnRequest, CancellationToken cancellationToken = default);
}
