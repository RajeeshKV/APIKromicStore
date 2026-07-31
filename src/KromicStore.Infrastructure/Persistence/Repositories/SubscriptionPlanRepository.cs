using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Subscription plan repository for tenant billing and feature tiers.
/// Supports plan creation, management, and tenant subscription assignment.
/// </summary>
public sealed class SubscriptionPlanRepository : ISubscriptionPlanRepository
{
    private readonly IApplicationDbContext _dbContext;

    public SubscriptionPlanRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<SubscriptionPlan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.MonthlyPrice)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SubscriptionPlan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SubscriptionPlans
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<SubscriptionPlan> Plans, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 20,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SubscriptionPlans.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var plans = await query
            .OrderBy(p => p.MonthlyPrice)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (plans, totalCount);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        var query = _dbContext.SubscriptionPlans
            .Where(p => p.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public void Add(SubscriptionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan, nameof(plan));
        _dbContext.AddEntity(plan);
    }

    public void Update(SubscriptionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan, nameof(plan));
        // Update is handled by EF Core tracking
    }

    public void Remove(SubscriptionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan, nameof(plan));
        // Soft delete via entity's SetDeleted or similar method if available
        // For now, mark as inactive
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
