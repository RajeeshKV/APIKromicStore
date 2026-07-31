using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

/// <summary>
/// Audit log repository for comprehensive activity tracking and compliance.
/// Records all entity changes, user actions, and system events for audit trails.
/// </summary>
public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly IApplicationDbContext _dbContext;

    public AuditLogRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .FirstOrDefaultAsync(al => al.Id == id, cancellationToken);
    }

    public async Task<(List<AuditLog> Logs, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 50,
        string? actionFilter = null,
        string? entityTypeFilter = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityTypeFilter))
        {
            var normalizedEntityType = entityTypeFilter.ToLowerInvariant();
            query = query.Where(al => al.EntityType.ToLower() == normalizedEntityType);
        }

        if (!string.IsNullOrWhiteSpace(actionFilter))
        {
            var normalizedAction = actionFilter.ToLowerInvariant();
            query = query.Where(al => al.Action.ToLower() == normalizedAction);
        }

        if (startDate.HasValue)
        {
            query = query.Where(al => al.OccurredOnUtc >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.AddDays(1).AddTicks(-1);
            query = query.Where(al => al.OccurredOnUtc <= endOfDay);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var logs = await query
            .OrderByDescending(al => al.OccurredOnUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (logs, totalCount);
    }

    public async Task<List<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType, nameof(entityType));

        var normalizedEntityType = entityType.ToLowerInvariant();
        return await _dbContext.AuditLogs
            .Where(al => al.EntityType.ToLower() == normalizedEntityType &&
                         al.EntityId == entityId)
            .OrderByDescending(al => al.OccurredOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetByActorAsync(
        Guid actorUserId,
        DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuditLogs
            .Where(al => al.ActorUserId == actorUserId);

        if (since.HasValue)
        {
            query = query.Where(al => al.OccurredOnUtc >= since.Value);
        }

        return await query
            .OrderByDescending(al => al.OccurredOnUtc)
            .ToListAsync(cancellationToken);
    }

    public void Add(AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log, nameof(log));
        _dbContext.AddEntity(log);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
