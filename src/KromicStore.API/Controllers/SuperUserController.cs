using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Tenants.Queries.GetPlatformDashboard;
using KromicStore.Application.Features.Tenants.Queries.GetTenants;
using KromicStore.Application.Features.Tenants.Queries.GetTenantDetail;

namespace KromicStore.API.Controllers;

/// <summary>
/// Super User (Platform Admin) API endpoints.
/// Provides access to platform management, tenant administration, and analytics.
/// </summary>
[ApiController]
[Route("api/v1/superuser")]
[Authorize(Roles = "SuperUser,PlatformAdmin")]
public class SuperUserController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuperUserController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets platform dashboard with key metrics.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetDashboard(CancellationToken cancellationToken = default)
    {
        var query = new GetPlatformDashboardQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets list of all tenants with filtering and pagination.
    /// </summary>
    [HttpGet("tenants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetTenants(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTenantsQuery
        {
            Skip = skip,
            Take = take,
            Status = status,
            Search = search
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets detailed information about a specific tenant.
    /// </summary>
    [HttpGet("tenants/{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetTenant(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTenantDetailQuery { TenantId = tenantId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
