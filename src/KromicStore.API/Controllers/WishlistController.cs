using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Shopping.Commands.AddToWishlist;
using KromicStore.Application.Features.Shopping.Commands.RemoveFromWishlist;
using KromicStore.Application.Features.Shopping.Queries.GetWishlist;
using KromicStore.Application.Features.Shopping.Queries.GetWishlistByCustomer;

namespace KromicStore.API.Controllers;

/// <summary>
/// Wishlist management API endpoints.
/// Allows authenticated customers to save products for later purchase.
/// </summary>
[ApiController]
[Route("api/v1/wishlist")]
[Produces("application/json")]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(IMediator mediator, ILogger<WishlistController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a wishlist by ID with all saved products.
    /// </summary>
    /// <param name="wishlistId">The wishlist ID.</param>
    /// <response code="200">Returns wishlist with items.</response>
    /// <response code="404">Wishlist not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{wishlistId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetWishlistResponse>> GetWishlist(
        Guid wishlistId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetWishlistQuery(wishlistId);
            var result = await _mediator.Send(query, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Wishlist not found: {WishlistId}", wishlistId);
                return NotFound(new { message = "Wishlist not found" });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized wishlist access: {WishlistId}", wishlistId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wishlist: {WishlistId}", wishlistId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving wishlist" });
        }
    }

    /// <summary>
    /// Gets the authenticated customer's wishlist.
    /// </summary>
    /// <response code="200">Returns customer's wishlist.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Wishlist not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet()]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetWishlistResponse>> GetMyWishlist(CancellationToken cancellationToken = default)
    {
        try
        {
            var customerId = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(customerId, out var customerIdGuid))
            {
                _logger.LogWarning("Invalid customer ID in claims");
                return Unauthorized(new { message = "Invalid user context" });
            }

            var query = new GetWishlistByCustomerQuery(customerIdGuid);
            var result = await _mediator.Send(query, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Wishlist not found for customer: {CustomerId}", customerIdGuid);
                return NotFound(new { message = "Wishlist not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer's wishlist");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving wishlist" });
        }
    }

    /// <summary>
    /// Adds a product to the customer's wishlist.
    /// </summary>
    /// <param name="request">Product ID to add.</param>
    /// <response code="200">Product added to wishlist.</response>
    /// <response code="400">Validation error or product already in wishlist.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("items")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetWishlistResponse>> AddToWishlist(
        [FromBody] AddToWishlistRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customerId = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(customerId, out var customerIdGuid))
            {
                _logger.LogWarning("Invalid customer ID in claims");
                return Unauthorized(new { message = "Invalid user context" });
            }

            if (request == null || request.ProductId == Guid.Empty)
            {
                _logger.LogWarning("Invalid add to wishlist request");
                return BadRequest(new { message = "ProductId is required" });
            }

            var command = new AddToWishlistCommand(
                request.WishlistId,
                request.ProductId,
                customerIdGuid);

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Wishlist operation failed");
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized wishlist access");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to wishlist");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error adding item" });
        }
    }

    /// <summary>
    /// Removes a product from the customer's wishlist.
    /// </summary>
    /// <param name="wishlistId">The wishlist ID.</param>
    /// <param name="productId">The product ID to remove.</param>
    /// <response code="200">Product removed from wishlist.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Wishlist or item not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{wishlistId}/items/{productId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetWishlistResponse>> RemoveFromWishlist(
        Guid wishlistId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customerId = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(customerId, out var customerIdGuid))
            {
                _logger.LogWarning("Invalid customer ID in claims");
                return Unauthorized(new { message = "Invalid user context" });
            }

            var command = new RemoveFromWishlistCommand(wishlistId, productId, customerIdGuid);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Remove from wishlist failed: {WishlistId}", wishlistId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {WishlistId}", wishlistId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from wishlist: {WishlistId}", wishlistId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

// ── Request/Response DTOs ───────────────────────────────────────────────────

public record AddToWishlistRequest(
    Guid WishlistId,
    Guid ProductId);
