namespace KromicStore.Application.Common.Repositories;

/// <summary>
/// Repository abstraction for fulfillments.
/// </summary>
public interface IFulfillmentRepository
{
    Task<Domain.StoreOperations.Entities.Fulfillment?> GetByIdAsync(Guid fulfillmentId, CancellationToken cancellationToken = default);
    Task<Domain.StoreOperations.Entities.Fulfillment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.StoreOperations.Entities.Fulfillment>> GetByStatusAsync(string status, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.StoreOperations.Entities.Fulfillment>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.StoreOperations.Entities.Fulfillment fulfillment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.StoreOperations.Entities.Fulfillment fulfillment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.StoreOperations.Entities.Fulfillment fulfillment, CancellationToken cancellationToken = default);
}
