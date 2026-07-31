using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Tenants.Queries.GetAuditLogs;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for audit log management.
/// SuperUsers can view comprehensive audit logs of platform and tenant actions.
/// </summary>
[ApiController]
[Route("api/v1/audit-logs")]
[Authorize(Roles = "SuperUser,TenantAdmin")]
public class AuditLogController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogController"/> class.
    /// </summary>
    public AuditLogController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets audit logs with optional filtering.
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 50).</param>
    /// <param name="actionFilter">Filter by action type (optional).</param>
    /// <param name="entityTypeFilter">Filter by entity type (optional).</param>
    /// <param name="startDate">Filter by start date (optional).</param>
    /// <param name="endDate">Filter by end date (optional).</param>
    /// <returns>List of audit logs.</returns>
    /// <response code="200">Returns list of audit logs.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetAuditLogsResponse>> GetAuditLogs(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? actionFilter = null,
        [FromQuery] string? entityTypeFilter = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery
        {
            Skip = skip,
            Take = take,
            ActionFilter = actionFilter,
            EntityTypeFilter = entityTypeFilter,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific audit log entry by ID.
    /// </summary>
    /// <param name="logId">The audit log ID.</param>
    /// <returns>Audit log details.</returns>
    /// <response code="200">Returns audit log details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Audit log not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{logId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuditLogDto>> GetAuditLog(
        Guid logId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        var log = result.Logs.FirstOrDefault(l => l.Id == logId);

        if (log == null)
            return NotFound();

        return Ok(log);
    }
}
