using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Tenants.Commands.CreateFeatureFlag;
using KromicStore.Application.Features.Tenants.Commands.AssignFeatureFlag;
using KromicStore.Domain.Tenants;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for feature flag management.
/// SuperUsers can create and manage feature flags for controlling feature rollout.
/// </summary>
[ApiController]
[Route("api/v1/feature-flags")]
[Authorize(Roles = "SuperUser")]
public class FeatureFlagController : ControllerBase
{
    private readonly IMediator _mediator;
    private static readonly Dictionary<Guid, FeatureFlagDto> _inMemoryFeatureFlags = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagController"/> class.
    /// </summary>
    public FeatureFlagController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all feature flags.
    /// </summary>
    /// <returns>List of feature flags.</returns>
    /// <response code="200">Returns list of feature flags.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<IEnumerable<FeatureFlagDto>>> GetFeatureFlags(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ActionResult<IEnumerable<FeatureFlagDto>>>(Ok(_inMemoryFeatureFlags.Values));
    }

    /// <summary>
    /// Creates a new feature flag.
    /// </summary>
    /// <param name="command">Feature flag creation command.</param>
    /// <returns>Created feature flag.</returns>
    /// <response code="201">Feature flag created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FeatureFlagDto>> CreateFeatureFlag(
        [FromBody] CreateFeatureFlagCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command, cancellationToken);

        var featureFlag = new FeatureFlagDto
        {
            Id = Guid.NewGuid(),
            Code = command.Code,
            Name = command.Name,
            Description = command.Description,
            IsEnabled = command.IsEnabled,
            Scope = command.Scope.ToString()
        };

        _inMemoryFeatureFlags[featureFlag.Id] = featureFlag;

        return CreatedAtAction(nameof(GetFeatureFlag), new { flagId = featureFlag.Id }, featureFlag);
    }

    /// <summary>
    /// Gets a specific feature flag by ID.
    /// </summary>
    /// <param name="flagId">The feature flag ID.</param>
    /// <returns>Feature flag details.</returns>
    /// <response code="200">Returns feature flag details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Feature flag not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{flagId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<FeatureFlagDto>> GetFeatureFlag(
        Guid flagId,
        CancellationToken cancellationToken = default)
    {
        if (!_inMemoryFeatureFlags.TryGetValue(flagId, out var flag))
            return Task.FromResult<ActionResult<FeatureFlagDto>>(NotFound());

        return Task.FromResult<ActionResult<FeatureFlagDto>>(Ok(flag));
    }

    /// <summary>
    /// Updates a feature flag.
    /// </summary>
    /// <param name="flagId">The feature flag ID.</param>
    /// <param name="request">Feature flag update request.</param>
    /// <returns>Updated feature flag.</returns>
    /// <response code="200">Feature flag updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Feature flag not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{flagId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<FeatureFlagDto>> UpdateFeatureFlag(
        Guid flagId,
        [FromBody] UpdateFeatureFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_inMemoryFeatureFlags.TryGetValue(flagId, out var flag))
            return Task.FromResult<ActionResult<FeatureFlagDto>>(NotFound());

        flag.IsEnabled = request.IsEnabled;
        flag.Name = request.Name ?? flag.Name;
        flag.Description = request.Description ?? flag.Description;

        return Task.FromResult<ActionResult<FeatureFlagDto>>(Ok(flag));
    }

    /// <summary>
    /// Deletes a feature flag.
    /// </summary>
    /// <param name="flagId">The feature flag ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Feature flag deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Feature flag not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{flagId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteFeatureFlag(
        Guid flagId,
        CancellationToken cancellationToken = default)
    {
        if (!_inMemoryFeatureFlags.Remove(flagId))
            return Task.FromResult<IActionResult>(NotFound());

        return Task.FromResult<IActionResult>(NoContent());
    }

    /// <summary>
    /// Assigns a feature flag to a tenant or subscription plan.
    /// </summary>
    /// <param name="flagId">The feature flag ID.</param>
    /// <param name="command">Feature flag assignment command.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Feature flag assigned successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Feature flag not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{flagId}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignFeatureFlag(
        Guid flagId,
        [FromBody] AssignFeatureFlagCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!_inMemoryFeatureFlags.ContainsKey(flagId))
            return NotFound();

        command.FeatureFlagId = flagId;
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
}

/// <summary>
/// DTO representing a feature flag.
/// </summary>
public class FeatureFlagDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public string Scope { get; set; } = string.Empty;
}

/// <summary>
/// Request to update a feature flag.
/// </summary>
public class UpdateFeatureFlagRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
}
