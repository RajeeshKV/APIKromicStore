using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, GetAuditLogsResponse>
{
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<GetAuditLogsQueryHandler> _logger;

    public GetAuditLogsQueryHandler(
        IAuditLogRepository auditRepository,
        ILogger<GetAuditLogsQueryHandler> logger)
    {
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetAuditLogsResponse> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving audit logs: Skip={Skip}, Take={Take}", request.Skip, request.Take);

        var (logs, totalCount) = await _auditRepository.GetPaginatedAsync(
            request.Skip,
            request.Take,
            request.ActionFilter,
            request.EntityTypeFilter,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var dtos = logs.Select(l => new AuditLogDto
        {
            Id = l.Id,
            Action = l.Action,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            EntityName = l.EntityName,
            ActorEmail = l.ActorEmail,
            OccurredOnUtc = l.OccurredOnUtc,
            Severity = l.Severity.ToString()
        }).ToList();

        return new GetAuditLogsResponse { Logs = dtos, TotalCount = totalCount };
    }
}
