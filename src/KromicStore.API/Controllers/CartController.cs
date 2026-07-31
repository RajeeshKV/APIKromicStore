using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Shopping.Commands.AddToCart;
using KromicStore.Application.Features.Shopping.Commands.UpdateCartItem;
using KromicStore.Application.Features.Shopping.Commands.RemoveCartItem;
using KromicStore.Application.Features.Shopping.Commands.ClearCart;
using KromicStore.Application.Features.Shopping.Commands.ApplyCoupon;
using KromicStore.Application.Features.Shopping.Commands.RemoveCoupon;
using KromicStore.Application.Features.Shopping.Queries.GetCart;
using KromicStore.Application.Features.Shopping.Queries.GetCartByCustomer;

namespace KromicStore.API.Controllers;

/// <summary>
/// Shopping cart management API endpoints.
/// Supports anonymous guests and authenticated customers.
/// All endpoints require valid tenant resolution.
/// </summary>
[ApiController]
[Route("api/v1/cart")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CartController> _logger;

    public CartController(IMediator mediator, ILogger<CartController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a shopping cart by ID with all items and totals.
    /// </summary>
    /// <param name="cartId">The cart ID.</param>
    /// <response code="200">Returns cart with items.</response>
    /// <response code="404">Cart not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{cartId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetCartResponse>> GetCart(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetCartQuery(cartId);
            var result = await _mediator.Send(query, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Cart not found: {CartId}", cartId);
                return NotFound(new { message = "Cart not found" });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized cart access: {CartId}", cartId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cart: {CartId}", cartId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving cart" });
        }
    }

    /// <summary>
    /// Gets the authenticated customer's shopping cart.
    /// </summary>
    /// <response code="200">Returns customer's cart.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Cart not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("my-cart")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetCartResponse>> GetMyCart(CancellationToken cancellationToken = default)
    {
        try
        {
            var customerId = User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(customerId, out var customerIdGuid))
            {
                _logger.LogWarning("Invalid customer ID in claims");
                return Unauthorized(new { message = "Invalid user context" });
            }

            var query = new GetCartByCustomerQuery(customerIdGuid);
            var result = await _mediator.Send(query, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Cart not found for customer: {CustomerId}", customerIdGuid);
                return NotFound(new { message = "Cart not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer's cart");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving cart" });
        }
    }

    /// <summary>
    /// Adds a product to the shopping cart or increases its quantity.
    /// </summary>
    /// <param name="cartId">The cart ID.</param>
    /// <param name="request">Product details and quantity.</param>
    /// <response code="200">Item added or quantity updated.</response>
    /// <response code="400">Validation error or invalid product.</response>
    /// <response code="404">Cart not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{cartId}/items")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddToCartResponse>> AddToCart(
        Guid cartId,
        [FromBody] AddToCartRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || request.ProductId == Guid.Empty)
            {
                _logger.LogWarning("Invalid add to cart request");
                return BadRequest(new { message = "ProductId is required" });
            }

            if (request.Quantity <= 0)
            {
                return BadRequest(new { message = "Quantity must be greater than 0" });
            }

            var command = new AddToCartCommand(
                cartId,
                request.ProductId,
                request.UnitPrice,
                request.Quantity,
                request.VariantId);

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cart operation failed: {CartId}", cartId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized cart access: {CartId}", cartId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart: {CartId}", cartId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error adding item to cart" });
        }
    }

    /// <summary>
    /// Updates the quantity of a cart item.
    /// </summary>
    /// <param name="cartId">The cart ID.</param>
    /// <param name="productId">The product ID.</param>
    /// <param name="request">New quantity (0 to remove).</param>
    /// <response code="200">Item quantity updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Cart or item not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{cartId}/items/{productId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UpdateCartItemResponse>> UpdateCartItem(
        Guid cartId,
        Guid productId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || request.Quantity < 0)
            {
                return BadRequest(new { message = "Quantity must be >= 0" });
            }

            var command = new UpdateCartItemCommand(
                cartId,
                productId,
                request.Quantity,
                request.VariantId);

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cart update failed: {CartId}", cartId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {CartId}", cartId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item: {CartId}", cartId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Removes a product from the shopping cart.
    /// </summary>
    /// <param name="cartId">The cart ID.</param>
    /// <param name="productId">The product ID to remove.</param>
    /// <response code="200">Item removed.</response>
    /// <response code="404">Cart or item not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{cartId}/items/{productId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RemoveCartItemResponse>> RemoveCartItem(
        Guid cartId,
        Guid productId,
        [FromQuery] Guid? variantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RemoveCartItemCommand(cartId, productId, variantId);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Remove item failed: {CartId}", cartId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {CartId}", cartId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cart item: {CartId}", cartId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Clears all items from the shopping cart.
    /// </summary>
    /// <param name="cartId">The cart ID.</param>
    /// <response code="204">Cart cleared.</response>
    /// <response code="404">Cart not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{cartId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearCart(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new ClearCartCommand(cartId);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Clear cart failed: {CartId}", cartId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {CartId}", cartId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart: {CartId}", cartId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Applies a coupon code to the shopping cart.
    /// </summary>
    /// <param name="cartId">The cart ID.</param>
    /// <param name="request">Coupon code to apply.</param>
    /// <response code="200">Coupon applied.</response>
    /// <response code="400">Invalid or expired coupon.</response>
    /// <response code="404">Cart not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{cartId}/coupons")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApplyCoupon(
        Guid cartId,
        [FromBody] ApplyCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.CouponCode))
            {
                return BadRequest(new { message = "Coupon code is required" });
            }

            var command = new ApplyCouponCommand(cartId, request.CouponCode);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(new { message = "Coupon applied", discountAmount = result.DiscountAmount });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Coupon application failed: {CartId}", cartId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {CartId}", cartId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying coupon to cart: {CartId}", cartId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Removes the applied coupon from the shopping cart.
    /// </summary>
    /// <param name="cartId">The cart ID.</param>
    /// <response code="200">Coupon removed.</response>
    /// <response code="404">Cart not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{cartId}/coupons")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveCoupon(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RemoveCouponCommand(cartId);
            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = "Coupon removed" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Coupon removal failed: {CartId}", cartId);
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {CartId}", cartId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing coupon from cart: {CartId}", cartId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

// ── Request/Response DTOs ───────────────────────────────────────────────────

public record AddToCartRequest(
    Guid ProductId,
    decimal UnitPrice,
    int Quantity = 1,
    Guid? VariantId = null);

public record UpdateCartItemRequest(
    int Quantity,
    Guid? VariantId = null);

public record ApplyCouponRequest(string CouponCode);
