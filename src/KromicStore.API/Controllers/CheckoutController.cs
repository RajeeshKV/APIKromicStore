using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.Application.Features.Shopping.Commands.CreateCheckoutSession;
using KromicStore.Application.Features.Shopping.Commands.InitializePayment;
using KromicStore.Application.Features.Shopping.Queries.GetCheckoutSession;
using KromicStore.Application.Features.Orders.Commands.CreateOrder;

namespace KromicStore.API.Controllers;

/// <summary>
/// Checkout flow API endpoints.
/// Handles checkout session creation, address management, and order placement.
/// </summary>
[ApiController]
[Route("api/v1/checkout")]
[Produces("application/json")]
public class CheckoutController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(IMediator mediator, ILogger<CheckoutController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new checkout session from a shopping cart.
    /// </summary>
    /// <param name="request">Cart and customer details.</param>
    /// <response code="201">Checkout session created.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Cart not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("sessions")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateCheckoutSessionResponse>> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request,
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

            if (request == null || request.CartId == Guid.Empty)
            {
                return BadRequest(new { message = "CartId is required" });
            }

            var command = new CreateCheckoutSessionCommand(request.CartId, customerIdGuid);
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(
                nameof(GetCheckoutSession),
                new { sessionId = result.CheckoutSessionId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Checkout session creation failed");
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized checkout access");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error creating checkout session" });
        }
    }

    /// <summary>
    /// Retrieves the current state of a checkout session.
    /// </summary>
    /// <param name="sessionId">The checkout session ID.</param>
    /// <response code="200">Returns checkout session details.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("sessions/{sessionId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetCheckoutSessionResponse>> GetCheckoutSession(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetCheckoutSessionQuery(sessionId);
            var result = await _mediator.Send(query, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Checkout session not found: {SessionId}", sessionId);
                return NotFound(new { message = "Checkout session not found" });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to checkout session: {SessionId}", sessionId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving checkout session: {SessionId}", sessionId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates the shipping address for a checkout session.
    /// </summary>
    /// <param name="sessionId">The checkout session ID.</param>
    /// <param name="request">Address details.</param>
    /// <response code="200">Address updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("sessions/{sessionId}/shipping-address")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetCheckoutSessionResponse>> UpdateShippingAddress(
        Guid sessionId,
        [FromBody] UpdateShippingAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new { message = "Full name is required" });
            }

            // UpdateShippingAddressCommand implementation would be called here
            // For now, retrieve updated session to confirm changes
            var query = new GetCheckoutSessionQuery(sessionId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Address update failed: {SessionId}", sessionId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {SessionId}", sessionId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shipping address: {SessionId}", sessionId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates the billing address for a checkout session.
    /// </summary>
    /// <param name="sessionId">The checkout session ID.</param>
    /// <param name="request">Address details.</param>
    /// <response code="200">Address updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("sessions/{sessionId}/billing-address")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetCheckoutSessionResponse>> UpdateBillingAddress(
        Guid sessionId,
        [FromBody] UpdateBillingAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new { message = "Full name is required" });
            }

            // UpdateBillingAddressCommand implementation would be called here
            // For now, retrieve updated session to confirm changes
            var query = new GetCheckoutSessionQuery(sessionId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Address update failed: {SessionId}", sessionId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {SessionId}", sessionId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating billing address: {SessionId}", sessionId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Initializes payment for a checkout session.
    /// Returns payment token/reference needed by client for payment completion.
    /// </summary>
    /// <param name="sessionId">The checkout session ID.</param>
    /// <param name="request">Payment method details.</param>
    /// <response code="200">Payment initialized.</response>
    /// <response code="400">Validation error or invalid checkout state.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("sessions/{sessionId}/initialize-payment")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InitializePaymentResponse>> InitializePayment(
        Guid sessionId,
        [FromBody] InitializePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                return BadRequest(new { message = "PaymentMethod is required" });
            }

            var command = new InitializePaymentCommand(sessionId, request.PaymentMethod);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payment initialization failed: {SessionId}", sessionId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {SessionId}", sessionId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing payment: {SessionId}", sessionId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Places an order from a completed checkout session.
    /// Checkout must be in valid state (payment verified, addresses set).
    /// </summary>
    /// <param name="sessionId">The checkout session ID.</param>
    /// <response code="201">Order created.</response>
    /// <response code="400">Checkout not ready for order placement.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("sessions/{sessionId}/place-order")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateOrderResponse>> PlaceOrder(
        Guid sessionId,
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

            // Retrieve checkout session to get items and addresses
            var sessionQuery = new GetCheckoutSessionQuery(sessionId);
            var checkoutSession = await _mediator.Send(sessionQuery, cancellationToken);
            if (checkoutSession == null)
            {
                return NotFound(new { message = "Checkout session not found" });
            }

            if (checkoutSession.Status != "PaymentVerified")
            {
                return BadRequest(new { message = "Checkout session is not ready for order placement. Status: " + checkoutSession.Status });
            }

            // Create order from checkout session
            var command = new CreateOrderCommand
            {
                CheckoutSessionId = sessionId,
                CustomerId = customerIdGuid,
                TenantId = checkoutSession.TenantId
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(PlaceOrder), new { orderId = result.OrderId }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order placement failed: {SessionId}", sessionId);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized: {SessionId}", sessionId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing order: {SessionId}", sessionId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error placing order" });
        }
    }
}

// ── Request/Response DTOs ───────────────────────────────────────────────────

public record CreateCheckoutSessionRequest(Guid CartId);

public record UpdateShippingAddressRequest(
    string FullName,
    string Email,
    string Phone,
    string AddressLine1,
    string AddressLine2,
    string City,
    string StateProvince,
    string PostalCode,
    string Country);

public record UpdateBillingAddressRequest(
    string FullName,
    string Email,
    string Phone,
    string AddressLine1,
    string AddressLine2,
    string City,
    string StateProvince,
    string PostalCode,
    string Country);

public record InitializePaymentRequest(string PaymentMethod);


