using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Catalog;
using KromicStore.Application.Features.Storefront.Queries.GetStoreInfo;
using KromicStore.Application.Features.Storefront.Queries.ListFeaturedProducts;
using GetProductsQuery = KromicStore.Application.Features.Catalog.Queries.GetProducts.GetProductsQuery;
using GetCategoriesQuery = KromicStore.Application.Features.Catalog.Queries.GetCategories.GetCategoriesQuery;
using GetProductByIdQuery = KromicStore.Application.Features.Catalog.Queries.GetProductById.GetProductByIdQuery;
using SearchProductsQuery = KromicStore.Application.Features.Catalog.Queries.SearchProducts.SearchProductsQuery;

namespace KromicStore.API.Controllers;

/// <summary>
/// Public storefront API endpoints for customers browsing the store.
/// No authentication required. Tenant resolved from Host header.
/// </summary>
[ApiController]
[Route("api/v1/storefront")]
[AllowAnonymous]
public class StorefrontController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorefrontController"/> class.
    /// </summary>
    public StorefrontController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets public store information (name, logo, description, etc.)
    /// </summary>
    /// <returns>Store information</returns>
    /// <response code="200">Returns store information.</response>
    /// <response code="400">Invalid tenant or store not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetStoreInfoResponse>> GetStoreInfo(CancellationToken cancellationToken = default)
    {
        var query = new GetStoreInfoQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all product categories for browsing
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>Paginated list of categories</returns>
    /// <response code="200">Returns categories.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery(skip, take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets all published products with optional filtering
    /// </summary>
    /// <param name="categoryId">Optional: Filter by category ID.</param>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns products.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetProducts(
        [FromQuery] Guid? categoryId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(skip, take, categoryId, Status: null);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets featured/top products
    /// </summary>
    /// <param name="take">Number of products to return (default: 12, max: 50).</param>
    /// <returns>List of featured products</returns>
    /// <response code="200">Returns featured products.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("featured-products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListFeaturedProductsResponse>> GetFeaturedProducts(
        [FromQuery] int take = 12,
        CancellationToken cancellationToken = default)
    {
        if (take > 50) take = 50;
        if (take < 1) take = 12;

        var query = new ListFeaturedProductsQuery(take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets product details by ID
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>Product details with variants and images</returns>
    /// <response code="200">Returns product details.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("products/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailDto>> GetProductDetails(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.Data == null)
            return NotFound();

        return Ok(result.Data);
    }

    /// <summary>
    /// Searches products by name, description, or tags
    /// </summary>
    /// <param name="query">Search term</param>
    /// <param name="categoryId">Optional: Filter by category ID.</param>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>Search results</returns>
    /// <response code="200">Returns search results.</response>
    /// <response code="400">Invalid parameters or empty search query.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("search")]
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
            return BadRequest("Search query cannot be empty");

        var searchQuery = new SearchProductsQuery(query, skip, take, categoryId);
        var result = await _mediator.Send(searchQuery, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets store policies (shipping, return, privacy, etc.)
    /// </summary>
    /// <returns>Store policies</returns>
    /// <response code="200">Returns policies.</response>
    /// <response code="404">Policies not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("policies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<StorePoliciesDto>> GetPolicies(CancellationToken cancellationToken = default)
    {
        // Placeholder - will be implemented in next phase
        return Task.FromResult<ActionResult<StorePoliciesDto>>(Ok(new StorePoliciesDto(
            ShippingPolicy: "Shipping policy content",
            ReturnPolicy: "Return policy content",
            PrivacyPolicy: "Privacy policy content",
            TermsOfService: "Terms of service content")));
    }
}

public record StorePoliciesDto(
    string? ShippingPolicy,
    string? ReturnPolicy,
    string? PrivacyPolicy,
    string? TermsOfService);
