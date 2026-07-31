namespace KromicStore.Application.Common.Repositories;

/// <summary>
/// Repository abstraction for inventory adjustments.
/// </summary>
public interface IInventoryAdjustmentRepository
{
    Task<Domain.StoreOperations.Entities.InventoryAdjustment?> GetByIdAsync(Guid adjustmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.StoreOperations.Entities.InventoryAdjustment>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.StoreOperations.Entities.InventoryAdjustment>> GetByStatusAsync(string status, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.StoreOperations.Entities.InventoryAdjustment>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.StoreOperations.Entities.InventoryAdjustment adjustment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.StoreOperations.Entities.InventoryAdjustment adjustment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.StoreOperations.Entities.InventoryAdjustment adjustment, CancellationToken cancellationToken = default);
}
