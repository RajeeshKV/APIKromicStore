using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Catalog;
using KromicStore.API.Controllers.BaseControllers;
using GetInventoryQuery = KromicStore.Application.Features.Catalog.Queries.GetInventory.GetInventoryQuery;
using AdjustInventoryCommand = KromicStore.Application.Features.Catalog.Commands.AdjustInventory.AdjustInventoryCommand;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for inventory management.
/// Only TenantAdmin and StoreManager roles can access these endpoints.
/// SuperAdmin will get 403 Forbidden.
/// </summary>
[Route("api/v1/tenant/inventory")]
public class InventoryController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryController"/> class.
    /// </summary>
    public InventoryController(IMediator mediator, ILogger<InventoryController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets inventory information for a specific product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The inventory details.</returns>
    /// <response code="200">Returns the inventory information.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InventoryDto>> GetInventory(Guid productId, CancellationToken cancellationToken = default)
    {
        var query = new GetInventoryQuery(productId);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.Data == null)
            return NotFound();
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Adjusts the inventory quantity for a product.
    /// </summary>
    /// <param name="request">The inventory adjustment request.</param>
    /// <returns>The updated inventory information.</returns>
    /// <response code="200">Inventory adjusted successfully.</response>
    /// <response code="400">Validation error (e.g., adjustment would result in negative quantity).</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("adjust")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InventoryDto>> AdjustInventory(
        [FromBody] AdjustInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AdjustInventoryCommand(
            request.ProductId,
            request.AdjustmentQuantity,
            request.Reason ?? "Manual adjustment");
        
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }
}

