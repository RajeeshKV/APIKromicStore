using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Controllers.BaseControllers;
using KromicStore.Application.Features.Tenants.Queries.GetAuditLogs;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: SuperAdmin only endpoints for audit log management.
/// Only SuperAdmin role can access.
/// TenantAdmin/StoreManager/Customer get 403.
/// Routes: /api/v1/super/audit-logs/*
/// </summary>
[Route("audit-logs")]
public class AuditLogController : SuperAdminBaseController
{
    private readonly IMediator _mediator;

    public AuditLogController(IMediator mediator, ILogger<AuditLogController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>Gets audit logs with optional filtering.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetAuditLogsResponse>> GetAuditLogs(
        [FromQuery] int       skip              = 0,
        [FromQuery] int       take              = 50,
        [FromQuery] string?   actionFilter      = null,
        [FromQuery] string?   entityTypeFilter  = null,
        [FromQuery] DateTime? startDate         = null,
        [FromQuery] DateTime? endDate           = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery
        {
            Skip             = skip,
            Take             = take,
            ActionFilter     = actionFilter,
            EntityTypeFilter = entityTypeFilter,
            StartDate        = startDate,
            EndDate          = endDate
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>Gets a specific audit log entry by ID.</summary>
    [HttpGet("{logId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuditLogDto>> GetAuditLog(Guid logId, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(), cancellationToken);
        var log = result.Logs.FirstOrDefault(l => l.Id == logId);

        if (log == null) return NotFound();
        return Ok(log);
    }
}
