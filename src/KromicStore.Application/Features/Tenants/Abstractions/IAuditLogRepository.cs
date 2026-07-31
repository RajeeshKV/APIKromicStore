using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Abstractions;

/// <summary>
/// Repository abstraction for AuditLog.
/// </summary>
public interface IAuditLogRepository
{
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<AuditLog> Logs, int TotalCount)> GetPaginatedAsync(
        int skip = 0,
        int take = 50,
        string? actionFilter = null,
        string? entityTypeFilter = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    Task<List<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
    Task<List<AuditLog>> GetByActorAsync(
        Guid actorUserId,
        DateTime? since = null,
        CancellationToken cancellationToken = default);
    void Add(AuditLog log);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
