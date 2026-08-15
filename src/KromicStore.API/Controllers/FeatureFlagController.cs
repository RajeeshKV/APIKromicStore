using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Controllers.BaseControllers;
using KromicStore.Application.Features.Tenants.Commands.CreateFeatureFlag;
using KromicStore.Application.Features.Tenants.Commands.AssignFeatureFlag;
using KromicStore.Domain.Tenants;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: SuperAdmin only endpoints for feature flag management.
/// Only SuperAdmin role can access.
/// TenantAdmin/StoreManager/Customer get 403.
/// Routes: /api/v1/super/feature-flags/*
/// </summary>
[Route("api/v1/super/feature-flags")]
public class FeatureFlagController : SuperAdminBaseController
{
    private readonly IMediator _mediator;
    private static readonly Dictionary<Guid, FeatureFlagDto> _inMemoryFeatureFlags = new();

    public FeatureFlagController(IMediator mediator, ILogger<FeatureFlagController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>Gets all feature flags.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<IEnumerable<FeatureFlagDto>>> GetFeatureFlags(CancellationToken cancellationToken = default)
        => Task.FromResult<ActionResult<IEnumerable<FeatureFlagDto>>>(Ok(_inMemoryFeatureFlags.Values));

    /// <summary>Creates a new feature flag.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FeatureFlagDto>> CreateFeatureFlag(
        [FromBody] CreateFeatureFlagCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command, cancellationToken);

        var flag = new FeatureFlagDto
        {
            Id          = Guid.NewGuid(),
            Code        = command.Code,
            Name        = command.Name,
            Description = command.Description,
            IsEnabled   = command.IsEnabled,
            Scope       = command.Scope.ToString()
        };

        _inMemoryFeatureFlags[flag.Id] = flag;
        return CreatedAtAction(nameof(GetFeatureFlag), new { flagId = flag.Id }, flag);
    }

    /// <summary>Gets a specific feature flag.</summary>
    [HttpGet("{flagId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<FeatureFlagDto>> GetFeatureFlag(Guid flagId, CancellationToken cancellationToken = default)
    {
        if (!_inMemoryFeatureFlags.TryGetValue(flagId, out var flag))
            return Task.FromResult<ActionResult<FeatureFlagDto>>(NotFound());
        return Task.FromResult<ActionResult<FeatureFlagDto>>(Ok(flag));
    }

    /// <summary>Updates a feature flag.</summary>
    [HttpPut("{flagId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<FeatureFlagDto>> UpdateFeatureFlag(Guid flagId, [FromBody] UpdateFeatureFlagRequest request, CancellationToken cancellationToken = default)
    {
        if (!_inMemoryFeatureFlags.TryGetValue(flagId, out var flag))
            return Task.FromResult<ActionResult<FeatureFlagDto>>(NotFound());

        flag.IsEnabled   = request.IsEnabled;
        flag.Name        = request.Name ?? flag.Name;
        flag.Description = request.Description ?? flag.Description;
        return Task.FromResult<ActionResult<FeatureFlagDto>>(Ok(flag));
    }

    /// <summary>Deletes a feature flag.</summary>
    [HttpDelete("{flagId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteFeatureFlag(Guid flagId, CancellationToken cancellationToken = default)
    {
        if (!_inMemoryFeatureFlags.Remove(flagId))
            return Task.FromResult<IActionResult>(NotFound());
        return Task.FromResult<IActionResult>(NoContent());
    }

    /// <summary>Assigns a feature flag to a tenant or plan.</summary>
    [HttpPost("{flagId}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignFeatureFlag(Guid flagId, [FromBody] AssignFeatureFlagCommand command, CancellationToken cancellationToken = default)
    {
        if (!_inMemoryFeatureFlags.ContainsKey(flagId))
            return NotFound();

        command.FeatureFlagId = flagId;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public class FeatureFlagDto
{
    public Guid    Id          { get; set; }
    public string  Code        { get; set; } = string.Empty;
    public string  Name        { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool    IsEnabled   { get; set; }
    public string  Scope       { get; set; } = string.Empty;
}

public class UpdateFeatureFlagRequest
{
    public string? Name        { get; set; }
    public string? Description { get; set; }
    public bool    IsEnabled   { get; set; }
}

