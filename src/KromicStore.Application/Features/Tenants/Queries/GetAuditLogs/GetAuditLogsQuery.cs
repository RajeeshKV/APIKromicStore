using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetAuditLogs;

public sealed class GetAuditLogsQuery : IRequest<GetAuditLogsResponse>
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
    public string? ActionFilter { get; set; }
    public string? EntityTypeFilter { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class AuditLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? EntityName { get; set; }
    public string? ActorEmail { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public sealed class GetAuditLogsResponse
{
    public List<AuditLogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
}
