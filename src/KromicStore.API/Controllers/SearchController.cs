using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Catalog;
using SearchProductsQuery = KromicStore.Application.Features.Catalog.Queries.SearchProducts.SearchProductsQuery;

namespace KromicStore.API.Controllers;

/// <summary>
/// API endpoints for product search functionality.
/// </summary>
[ApiController]
[Route("api/v1/search")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchController"/> class.
    /// </summary>
    public SearchController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Searches for products by name, description, and attributes.
    /// </summary>
    /// <param name="query">The search query text.</param>
    /// <param name="categoryId">Optional: Filter by category ID.</param>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>A list of matching products.</returns>
    /// <response code="200">Returns the search results.</response>
    /// <response code="400">Validation error (e.g., query too short).</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("products")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductSearchResultDto>>> SearchProducts(
        [FromQuery] string? query,
        [FromQuery] Guid? categoryId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Search query is required and must not be empty." });
        }

        if (query.Length < 2)
        {
            return BadRequest(new { error = "Search query must be at least 2 characters long." });
        }

        if (take > 100)
        {
            take = 100;
        }

        var searchQuery = new SearchProductsQuery(query, skip, take, categoryId);
        var result = await _mediator.Send(searchQuery, cancellationToken);
        return Ok(result.Data);
    }
}
