using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Shipping;
using KromicStore.API.Controllers.BaseControllers;
using CreateShippingZoneCommand = KromicStore.Application.Features.Shipping.Commands.CreateShippingZone.CreateShippingZoneCommand;
using AddShippingMethodCommand = KromicStore.Application.Features.Shipping.Commands.AddShippingMethod.AddShippingMethodCommand;
using CalculateShippingCostCommand = KromicStore.Application.Features.Shipping.Commands.CalculateShippingCost.CalculateShippingCostCommand;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for shipping zone and method management.
/// Only TenantAdmin and StoreManager roles can access.
/// SuperAdmin gets 403.
/// Routes: /api/v1/tenant/shipping/*
/// </summary>
[Route("api/v1/tenant/shipping")]
public class ShippingManagementController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShippingManagementController"/> class.
    /// </summary>
    public ShippingManagementController(IMediator mediator, ILogger<ShippingManagementController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Creates a new shipping zone for the tenant's store.
    /// Zones define geographic regions for shipping calculation.
    /// </summary>
    /// <param name="request">Shipping zone creation request.</param>
    /// <returns>Created shipping zone details.</returns>
    /// <response code="201">Zone created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("zones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<ShippingZoneDto>> CreateShippingZone(
        [FromBody] CreateShippingZoneRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateShippingZoneCommand
        {
            Name = request.Name,
            Description = request.Description
        };

        var response = _mediator.Send(command, cancellationToken).Result;

        var zoneDto = new ShippingZoneDto
        {
            ZoneId = response.ZoneId,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return Task.FromResult<ActionResult<ShippingZoneDto>>(CreatedAtAction(nameof(GetShippingZone), new { zoneId = response.ZoneId }, zoneDto));
    }

    /// <summary>
    /// Gets a specific shipping zone by ID.
    /// </summary>
    /// <param name="zoneId">The shipping zone ID.</param>
    /// <returns>Shipping zone details.</returns>
    /// <response code="200">Returns zone details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Zone not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("zones/{zoneId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<ShippingZoneDto>> GetShippingZone(
        Guid zoneId,
        CancellationToken cancellationToken = default)
    {
        // Get zone handler would retrieve from DB
        return Task.FromResult<ActionResult<ShippingZoneDto>>(NotFound());
    }

    /// <summary>
    /// Gets all shipping zones for the tenant's store.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <returns>List of shipping zones.</returns>
    /// <response code="200">Returns list of zones.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("zones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<IEnumerable<ShippingZoneDto>>> GetShippingZones(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        // Get zones handler would retrieve list from DB
        return Task.FromResult<ActionResult<IEnumerable<ShippingZoneDto>>>(Ok(Enumerable.Empty<ShippingZoneDto>()));
    }

    /// <summary>
    /// Updates an existing shipping zone.
    /// </summary>
    /// <param name="zoneId">The shipping zone ID.</param>
    /// <param name="request">Zone update request.</param>
    /// <returns>Updated zone details.</returns>
    /// <response code="200">Zone updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Zone not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("zones/{zoneId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<ShippingZoneDto>> UpdateShippingZone(
        Guid zoneId,
        [FromBody] UpdateShippingZoneRequest request,
        CancellationToken cancellationToken = default)
    {
        // Update zone handler would be sent here
        return Task.FromResult<ActionResult<ShippingZoneDto>>(NotFound());
    }

    /// <summary>
    /// Deletes a shipping zone.
    /// </summary>
    /// <param name="zoneId">The shipping zone ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Zone deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Zone not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("zones/{zoneId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteShippingZone(
        Guid zoneId,
        CancellationToken cancellationToken = default)
    {
        // Delete zone handler would be sent here
        return Task.FromResult<IActionResult>(NoContent());
    }

    /// <summary>
    /// Adds a new shipping method to a shipping zone.
    /// </summary>
    /// <param name="zoneId">The shipping zone ID.</param>
    /// <param name="request">Shipping method creation request.</param>
    /// <returns>Created shipping method details.</returns>
    /// <response code="201">Method created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Zone not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("zones/{zoneId}/methods")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<ShippingMethodDto>> AddShippingMethod(
        Guid zoneId,
        [FromBody] CreateShippingMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddShippingMethodCommand
        {
            ShippingZoneId = zoneId,
            Name = request.Name,
            Code = request.Name.ToUpperInvariant().Replace(" ", "-"),
            EstimatedDaysMin = request.EstimatedDays,
            EstimatedDaysMax = request.EstimatedDays + 1
        };

        var response = _mediator.Send(command, cancellationToken).Result;

        var methodDto = new ShippingMethodDto
        {
            MethodId = response.MethodId,
            ShippingZoneId = response.ZoneId,
            Name = request.Name,
            Carrier = "Standard",
            BaseRate = request.BaseRate,
            EstimatedDays = request.EstimatedDays,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return Task.FromResult<ActionResult<ShippingMethodDto>>(CreatedAtAction(nameof(GetShippingMethod), new { zoneId, methodId = response.MethodId }, methodDto));
    }

    /// <summary>
    /// Gets a specific shipping method.
    /// </summary>
    /// <param name="zoneId">The shipping zone ID.</param>
    /// <param name="methodId">The shipping method ID.</param>
    /// <returns>Shipping method details.</returns>
    /// <response code="200">Returns method details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Method not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("zones/{zoneId}/methods/{methodId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<ShippingMethodDto>> GetShippingMethod(
        Guid zoneId,
        Guid methodId,
        CancellationToken cancellationToken = default)
    {
        // Get method handler would retrieve from DB
        return Task.FromResult<ActionResult<ShippingMethodDto>>(NotFound());
    }

    /// <summary>
    /// Gets all shipping methods for a zone.
    /// </summary>
    /// <param name="zoneId">The shipping zone ID.</param>
    /// <returns>List of shipping methods.</returns>
    /// <response code="200">Returns list of methods.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Zone not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("zones/{zoneId}/methods")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<IEnumerable<ShippingMethodDto>>> GetShippingMethods(
        Guid zoneId,
        CancellationToken cancellationToken = default)
    {
        // Get methods handler would retrieve list from DB
        return Task.FromResult<ActionResult<IEnumerable<ShippingMethodDto>>>(Ok(Enumerable.Empty<ShippingMethodDto>()));
    }

    /// <summary>
    /// Updates an existing shipping method.
    /// </summary>
    /// <param name="zoneId">The shipping zone ID.</param>
    /// <param name="methodId">The shipping method ID.</param>
    /// <param name="request">Method update request.</param>
    /// <returns>Updated method details.</returns>
    /// <response code="200">Method updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Method not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("zones/{zoneId}/methods/{methodId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<ShippingMethodDto>> UpdateShippingMethod(
        Guid zoneId,
        Guid methodId,
        [FromBody] UpdateShippingMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        // Update method handler would be sent here
        return Task.FromResult<ActionResult<ShippingMethodDto>>(NotFound());
    }

    /// <summary>
    /// Deletes a shipping method.
    /// </summary>
    /// <param name="zoneId">The shipping zone ID.</param>
    /// <param name="methodId">The shipping method ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Method deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Method not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("zones/{zoneId}/methods/{methodId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> DeleteShippingMethod(
        Guid zoneId,
        Guid methodId,
        CancellationToken cancellationToken = default)
    {
        // Delete method handler would be sent here
        return Task.FromResult<IActionResult>(NoContent());
    }

    /// <summary>
    /// Calculates shipping cost for given method and destination.
    /// </summary>
    /// <param name="methodId">The shipping method ID.</param>
    /// <param name="request">Calculation request parameters.</param>
    /// <returns>Calculated shipping cost.</returns>
    /// <response code="200">Cost calculated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Method not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("calculate")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult> CalculateShippingCost(
        [FromQuery] Guid methodId,
        [FromBody] dynamic request,
        CancellationToken cancellationToken = default)
    {
        var command = new CalculateShippingCostCommand
        {
            ShippingMethodId = methodId
        };

        var response = _mediator.Send(command, cancellationToken).Result;

        if (!response.Success)
            return Task.FromResult<ActionResult>(BadRequest(new { message = "Unable to calculate shipping cost." }));

        return Task.FromResult<ActionResult>(Ok(new { cost = response.ShippingCost }));
    }
}

