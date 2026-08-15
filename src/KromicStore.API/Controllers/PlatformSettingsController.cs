using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Controllers.BaseControllers;
using KromicStore.Application.Features.Tenants.Queries.GetPlatformSettings;
using KromicStore.Application.Features.Tenants.Commands.UpdatePlatformSettings;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: SuperAdmin only endpoints for platform-wide settings.
/// Only SuperAdmin role can access.
/// TenantAdmin/StoreManager/Customer get 403.
/// Routes: /api/v1/super/platform-settings
/// </summary>
[Route("platform-settings")]
public class PlatformSettingsController : SuperAdminBaseController
{
    private readonly IMediator _mediator;

    public PlatformSettingsController(IMediator mediator, ILogger<PlatformSettingsController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>Gets platform-wide settings.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlatformSettingsDto>> GetPlatformSettings(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPlatformSettingsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Updates platform-wide settings.</summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlatformSettingsDto>> UpdatePlatformSettings(
        [FromBody] UpdatePlatformSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command, cancellationToken);
        var result = await _mediator.Send(new GetPlatformSettingsQuery(), cancellationToken);
        return Ok(result);
    }
}
