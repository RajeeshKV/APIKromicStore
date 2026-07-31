using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Tenants.Queries.GetPlatformSettings;
using KromicStore.Application.Features.Tenants.Commands.UpdatePlatformSettings;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for platform-wide settings management.
/// Only SuperUsers can view and modify platform settings.
/// </summary>
[ApiController]
[Route("api/v1/platform-settings")]
[Authorize(Roles = "SuperUser")]
public class PlatformSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformSettingsController"/> class.
    /// </summary>
    public PlatformSettingsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets platform-wide settings.
    /// </summary>
    /// <returns>Platform settings.</returns>
    /// <response code="200">Returns platform settings.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlatformSettingsDto>> GetPlatformSettings(
        CancellationToken cancellationToken = default)
    {
        var query = new GetPlatformSettingsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates platform-wide settings.
    /// </summary>
    /// <param name="command">Platform settings update command.</param>
    /// <returns>Updated platform settings.</returns>
    /// <response code="200">Platform settings updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
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
        
        // Fetch and return updated settings
        var query = new GetPlatformSettingsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
