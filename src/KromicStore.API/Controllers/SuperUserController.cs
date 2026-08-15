using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Controllers.BaseControllers;
using KromicStore.Application.Features.Tenants.Queries.GetPlatformDashboard;
using KromicStore.Application.Features.Tenants.Queries.GetTenants;
using KromicStore.Application.Features.Tenants.Queries.GetTenantDetail;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: SuperAdmin only endpoints for platform management.
/// Only SuperAdmin role can access.
/// TenantAdmin/StoreManager/Customer get 403.
/// Routes: /api/v1/super/platform/*
/// </summary>
[Route("platform")]
public class SuperUserController : SuperAdminBaseController
{
    private readonly IMediator _mediator;

    public SuperUserController(IMediator mediator, ILogger<SuperUserController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>Gets platform dashboard with key metrics.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetDashboard(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPlatformDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets list of all tenants with filtering and pagination.</summary>
    [HttpGet("tenants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetTenants(
        [FromQuery] int     skip   = 0,
        [FromQuery] int     take   = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTenantsQuery { Skip = skip, Take = take, Status = status, Search = search }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets detailed information about a specific tenant.</summary>
    [HttpGet("tenants/{tenantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetTenant(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTenantDetailQuery { TenantId = tenantId }, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
