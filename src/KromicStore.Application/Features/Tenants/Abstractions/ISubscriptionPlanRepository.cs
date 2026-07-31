using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Abstractions;

/// <summary>
/// Repository abstraction for SubscriptionPlan aggregate.
/// </summary>
public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<(List<SubscriptionPlan> Plans, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 20,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    void Add(SubscriptionPlan plan);
    void Update(SubscriptionPlan plan);
    void Remove(SubscriptionPlan plan);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
