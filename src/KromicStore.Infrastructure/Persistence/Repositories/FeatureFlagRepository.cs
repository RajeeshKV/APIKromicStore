using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Feature flag repository for progressive rollout and tenant-specific features.
/// Manages feature flags and their assignment to specific tenants.
/// </summary>
public sealed class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly IApplicationDbContext _dbContext;

    public FeatureFlagRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<FeatureFlag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FeatureFlags
            .FirstOrDefaultAsync(ff => ff.Id == id, cancellationToken);
    }

    public async Task<FeatureFlag?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));

        var normalizedCode = code.ToLowerInvariant();
        return await _dbContext.FeatureFlags
            .FirstOrDefaultAsync(ff => ff.Code.ToLower() == normalizedCode, cancellationToken);
    }

    public async Task<List<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FeatureFlags
            .OrderBy(ff => ff.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FeatureFlag>> GetEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FeatureFlags
            .Where(ff => ff.IsEnabled)
            .OrderBy(ff => ff.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<FeatureFlag> Flags, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 20,
        bool? isEnabled = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FeatureFlags.AsQueryable();

        if (isEnabled.HasValue)
        {
            query = query.Where(ff => ff.IsEnabled == isEnabled.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var flags = await query
            .OrderBy(ff => ff.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (flags, totalCount);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));

        var normalizedCode = code.ToLowerInvariant();
        var query = _dbContext.FeatureFlags
            .Where(ff => ff.Code.ToLower() == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(ff => ff.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public void Add(FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag, nameof(flag));
        _dbContext.AddEntity(flag);
    }

    public void Update(FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag, nameof(flag));
        // Update is handled by EF Core tracking
    }

    public void Remove(FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag, nameof(flag));
        // Soft delete via entity's Disable or similar method
        flag.Disable();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
