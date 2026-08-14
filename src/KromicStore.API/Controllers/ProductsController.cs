using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Catalog;
using GetProductsQuery = KromicStore.Application.Features.Catalog.Queries.GetProducts.GetProductsQuery;
using GetProductByIdQuery = KromicStore.Application.Features.Catalog.Queries.GetProductById.GetProductByIdQuery;
using CreateProductCommand = KromicStore.Application.Features.Catalog.Commands.CreateProduct.CreateProductCommand;
using UpdateProductCommand = KromicStore.Application.Features.Catalog.Commands.UpdateProduct.UpdateProductCommand;
using DeleteProductCommand = KromicStore.Application.Features.Catalog.Commands.DeleteProduct.DeleteProductCommand;
using RestoreProductCommand = KromicStore.Application.Features.Catalog.Commands.RestoreProduct.RestoreProductCommand;
using DuplicateProductCommand = KromicStore.Application.Features.Catalog.Commands.DuplicateProduct.DuplicateProductCommand;
using BulkDeleteProductsCommand = KromicStore.Application.Features.Catalog.Commands.BulkDeleteProducts.BulkDeleteProductsCommand;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for product management.
/// </summary>
[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    public ProductsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all products with optional filtering and pagination.
    /// </summary>
    /// <param name="categoryId">Optional: Filter by category ID.</param>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>A paginated list of products.</returns>
    /// <response code="200">Returns the list of products.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetProducts(
        [FromQuery] Guid? categoryId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(skip, take, categoryId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <returns>The created product.</returns>
    /// <response code="201">Product created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="409">SKU already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailDto>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateProductCommand(
            request.CategoryId,
            request.Name,
            request.Sku,
            request.Slug,
            null,
            request.Description,
            null,
            "Draft",
            request.BasePrice,
            null,
            request.CostPrice,
            null,
            null,
            null,
            null,
            false,
            true,
            true,
            request.Attributes,
            request.Tags);
        
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, result);
    }

    /// <summary>
    /// Gets a specific product by ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product details.</returns>
    /// <response code="200">Returns the product.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.Data == null)
            return NotFound();
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="request">The product update request.</param>
    /// <returns>The updated product.</returns>
    /// <response code="200">Product updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="409">Conflict (e.g., SKU already exists).</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailDto>> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProductCommand(
            id,
            request.CategoryId,
            request.Name,
            null,
            request.Slug,
            null,
            request.Description,
            null,
            request.BasePrice,
            null,
            request.CostPrice);
        
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Soft-deletes a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Product deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Restores a soft-deleted product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The restored product.</returns>
    /// <response code="200">Product restored successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/restore")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailDto>> RestoreProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new RestoreProductCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Duplicates an existing product.
    /// </summary>
    /// <param name="id">The product ID to duplicate.</param>
    /// <param name="request">The duplication request parameters.</param>
    /// <returns>The duplicated product.</returns>
    /// <response code="201">Product duplicated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="409">SKU already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/duplicate")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailDto>> DuplicateProduct(
        Guid id,
        [FromBody] DuplicateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var newSku = request.NewSku ?? $"{id}-copy";
        var newName = request.NewName ?? "Copy";
        var command = new DuplicateProductCommand(id, newSku, newName);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = result.DuplicatedProductId }, result);
    }

    /// <summary>
    /// Bulk deletes multiple products.
    /// Soft deletes all specified products in a single efficient operation.
    /// </summary>
    /// <param name="request">List of product IDs to delete.</param>
    /// <returns>Operation result with success/failure counts.</returns>
    /// <response code="200">Bulk delete completed.</response>
    /// <response code="400">Validation error or empty list.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("bulk-delete")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> BulkDeleteProducts(
        [FromBody] BulkDeleteProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request?.ProductIds == null || !request.ProductIds.Any())
            return BadRequest(new { message = "No product IDs provided" });

        var command = new BulkDeleteProductsCommand(request.ProductIds);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }
}

// Request DTOs
public sealed record BulkDeleteProductsRequest(
    IEnumerable<Guid> ProductIds
);
