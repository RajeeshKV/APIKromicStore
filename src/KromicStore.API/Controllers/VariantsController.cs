using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Catalog;
using KromicStore.API.Controllers.BaseControllers;
using GetVariantsQuery = KromicStore.Application.Features.Catalog.Queries.GetVariants.GetVariantsQuery;
using CreateVariantCommand = KromicStore.Application.Features.Catalog.Commands.CreateVariant.CreateVariantCommand;
using UpdateVariantCommand = KromicStore.Application.Features.Catalog.Commands.UpdateVariant.UpdateVariantCommand;
using DeleteVariantCommand = KromicStore.Application.Features.Catalog.Commands.DeleteVariant.DeleteVariantCommand;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for product variant management.
/// Only TenantAdmin and StoreManager roles can access.
/// SuperAdmin gets 403.
/// Routes: /api/v1/tenant/products/{productId}/variants/*
/// </summary>
[Route("api/v1/tenant/products/{productId}/variants")]
public class VariantsController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    public VariantsController(IMediator mediator, ILogger<VariantsController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all variants for a specific product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>A list of product variants.</returns>
    /// <response code="200">Returns the list of variants.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<VariantDto>>> GetVariants(Guid productId, CancellationToken cancellationToken = default)
    {
        var query = new GetVariantsQuery(productId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new product variant.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="request">The variant creation request.</param>
    /// <returns>The created variant.</returns>
    /// <response code="201">Variant created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="409">SKU already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VariantDto>> CreateVariant(
        Guid productId,
        [FromBody] CreateVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateVariantCommand(
            productId,
            request.Sku,
            request.Name ?? "Variant",
            request.Price ?? 0,
            request.QuantityOnHand,
            request.Attributes);
        
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVariants), new { productId }, result);
    }

    /// <summary>
    /// Updates an existing product variant.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="variantId">The variant ID.</param>
    /// <param name="request">The variant update request.</param>
    /// <returns>The updated variant.</returns>
    /// <response code="200">Variant updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product or variant not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{variantId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VariantDto>> UpdateVariant(
        Guid productId,
        Guid variantId,
        [FromBody] UpdateVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateVariantCommand(
            productId,
            variantId,
            request.Name,
            null,
            request.Attributes,
            request.IsAvailable);
        
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Deletes a product variant.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="variantId">The variant ID.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Variant deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product or variant not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{variantId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteVariant(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteVariantCommand(productId, variantId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

