using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Abstractions;

/// <summary>
/// Repository abstraction for FeatureFlag aggregate.
/// </summary>
public interface IFeatureFlagRepository
{
    Task<FeatureFlag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeatureFlag?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<FeatureFlag>> GetEnabledAsync(CancellationToken cancellationToken = default);
    Task<(List<FeatureFlag> Flags, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 20,
        bool? isEnabled = null,
        CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    void Add(FeatureFlag flag);
    void Update(FeatureFlag flag);
    void Remove(FeatureFlag flag);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
