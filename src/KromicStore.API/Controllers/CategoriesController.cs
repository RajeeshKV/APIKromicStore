using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Catalog;
using KromicStore.API.Controllers.BaseControllers;
using GetCategoriesQuery = KromicStore.Application.Features.Catalog.Queries.GetCategories.GetCategoriesQuery;
using GetCategoryByIdQuery = KromicStore.Application.Features.Catalog.Queries.GetCategoryById.GetCategoryByIdQuery;
using CreateCategoryCommand = KromicStore.Application.Features.Catalog.Commands.CreateCategory.CreateCategoryCommand;
using UpdateCategoryCommand = KromicStore.Application.Features.Catalog.Commands.UpdateCategory.UpdateCategoryCommand;
using DeleteCategoryCommand = KromicStore.Application.Features.Catalog.Commands.DeleteCategory.DeleteCategoryCommand;
using RestoreCategoryCommand = KromicStore.Application.Features.Catalog.Commands.RestoreCategory.RestoreCategoryCommand;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for category management.
/// Only TenantAdmin and StoreManager roles can access these endpoints.
/// SuperAdmin will get 403 Forbidden.
/// </summary>
[Route("api/v1/tenant/categories")]
public class CategoriesController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoriesController"/> class.
    /// </summary>
    public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all product categories.
    /// </summary>
    /// <returns>A paginated list of categories.</returns>
    /// <response code="200">Returns the list of categories.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] Guid? parentCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery(skip, take, parentCategoryId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new product category.
    /// </summary>
    /// <param name="request">The category creation request.</param>
    /// <returns>The created category.</returns>
    /// <response code="201">Category created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="409">Category with this slug already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCategoryCommand(
            request.Name,
            request.Description,
            request.Slug,
            request.ParentCategoryId,
            request.DisplayOrder,
            request.IsActive);
        
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategory), new { id = result.CategoryId }, result);
    }

    /// <summary>
    /// Gets a specific product category by ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The category details.</returns>
    /// <response code="200">Returns the category.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.Data == null)
            return NotFound();
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Updates an existing product category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="request">The category update request.</param>
    /// <returns>The updated category.</returns>
    /// <response code="200">Category updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="409">Conflict (e.g., slug already exists).</response>
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
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCategoryCommand(
            id,
            request.Name,
            request.Description,
            request.Slug,
            request.ParentCategoryId,
            request.DisplayOrder,
            null,
            null);
        
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Soft-deletes a product category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Category deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Restores a soft-deleted product category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The restored category.</returns>
    /// <response code="200">Category restored successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Category not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id}/restore")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> RestoreCategory(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new RestoreCategoryCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }
}

