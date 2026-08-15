using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Catalog;
using KromicStore.API.Controllers.BaseControllers;
using GetCollectionsQuery = KromicStore.Application.Features.Catalog.Queries.GetCollections.GetCollectionsQuery;
using GetCollectionByIdQuery = KromicStore.Application.Features.Catalog.Queries.GetCollectionById.GetCollectionByIdQuery;
using CreateCollectionCommand = KromicStore.Application.Features.Catalog.Commands.CreateCollection.CreateCollectionCommand;
using UpdateCollectionCommand = KromicStore.Application.Features.Catalog.Commands.UpdateCollection.UpdateCollectionCommand;
using DeleteCollectionCommand = KromicStore.Application.Features.Catalog.Commands.DeleteCollection.DeleteCollectionCommand;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for product collection management.
/// Only TenantAdmin and StoreManager roles can access these endpoints.
/// SuperAdmin will get 403 Forbidden.
/// </summary>
[Route("collections")]
public class CollectionsController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionsController"/> class.
    /// </summary>
    public CollectionsController(IMediator mediator, ILogger<CollectionsController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all active product collections.
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>A paginated list of collections.</returns>
    /// <response code="200">Returns the list of collections.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CollectionDto>>> GetCollections(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCollectionsQuery(skip, take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Creates a new product collection.
    /// </summary>
    /// <param name="request">The collection creation request.</param>
    /// <returns>The created collection.</returns>
    /// <response code="201">Collection created successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="409">Collection with this slug already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionDto>> CreateCollection(
        [FromBody] CreateCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCollectionCommand(
            request.Name,
            request.Description,
            0,
            "Active");
        
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCollection), new { id = result.CollectionId }, result);
    }

    /// <summary>
    /// Gets a specific product collection by ID.
    /// </summary>
    /// <param name="id">The collection ID.</param>
    /// <returns>The collection details.</returns>
    /// <response code="200">Returns the collection.</response>
    /// <response code="404">Collection not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionDto>> GetCollection(Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetCollectionByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.Data == null)
            return NotFound();
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Updates an existing product collection.
    /// </summary>
    /// <param name="id">The collection ID.</param>
    /// <param name="request">The collection update request.</param>
    /// <returns>The updated collection.</returns>
    /// <response code="200">Collection updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Collection not found.</response>
    /// <response code="409">Conflict (e.g., slug already exists).</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionDto>> UpdateCollection(
        Guid id,
        [FromBody] UpdateCollectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCollectionCommand(
            id,
            request.Name,
            request.Description,
            null,
            request.IsActive ? "Active" : "Inactive");
        
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Deletes a product collection.
    /// </summary>
    /// <param name="id">The collection ID.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Collection deleted successfully.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="404">Collection not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCollection(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteCollectionCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
