using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Controllers.BaseControllers;
using KromicStore.Application.Features.Orders.Commands.ConfirmOrder;
using KromicStore.Application.Features.Orders.Commands.RejectOrder;
using KromicStore.Application.Features.Orders.Commands.CancelOrder;
using KromicStore.Application.Features.Orders.Commands.AddShipment;
using KromicStore.Application.Features.Orders.Commands.BulkUpdateOrderStatus;
using KromicStore.Application.Features.Orders.Queries.GetOrders;
using KromicStore.Application.Features.Orders.Queries.GetOrderById;
using KromicStore.Application.Features.Orders.Queries.GetTracking;
using KromicStore.Application.Features.Orders.Queries.ExportOrders;
using GetProductsQuery = KromicStore.Application.Features.Catalog.Queries.GetProducts.GetProductsQuery;

namespace KromicStore.API.Controllers;

/// <summary>
/// STRICT: Tenant Admin endpoints for order management.
/// Only TenantAdmin and StoreManager roles can access these endpoints.
/// SuperAdmin will get 403 Forbidden.
/// </summary>
[Route("api/v1/tenant/orders")]
public class OrdersController : TenantAdminBaseController
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger) : base(logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all orders for the current user (customer's orders or tenant's store orders).
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <param name="status">Optional: Filter by order status (e.g., "Pending", "Confirmed", "Dispatched").</param>
    /// <returns>Paginated list of orders</returns>
    /// <response code="200">Returns orders.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrders(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        // Get current customer ID from claims
        var customerIdClaim = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(customerIdClaim, out var customerId))
            return Unauthorized();

        var query = new GetOrdersQuery
        {
            CustomerId = customerId,
            Status = status,
            Skip = skip,
            Take = take
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Orders);
    }

    /// <summary>
    /// Gets a specific order by ID.
    /// Customers can only see their own orders; tenants can see their store orders.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>Order details with items and timeline</returns>
    /// <response code="200">Returns order details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden (not your order).</response>
    /// <response code="404">Order not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{orderId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // Get current customer ID from claims
        var customerIdClaim = User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(customerIdClaim, out var customerId))
            return Unauthorized();

        var query = new GetOrderByIdQuery
        {
            OrderId = orderId,
            CustomerId = customerId
        };

        var result = await _mediator.Send(query, cancellationToken);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Confirms an order (tenant only).
    /// Changes order status from Pending to Confirmed and triggers notifications.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>Updated order details</returns>
    /// <response code="200">Order confirmed.</response>
    /// <response code="400">Invalid order state for confirmation.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden (not your store).</response>
    /// <response code="404">Order not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{orderId}/confirm")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDetailDto>> ConfirmOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // Get tenant ID from claims
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var command = new ConfirmOrderCommand { OrderId = orderId, TenantId = tenantId };
        var result = await _mediator.Send(command, cancellationToken);

        if (result == null)
            return NotFound();

        // Retrieve full order details for response
        var query = new GetOrderByIdQuery
        {
            OrderId = orderId,
            TenantId = tenantId
        };

        var orderDetail = await _mediator.Send(query, cancellationToken);
        if (orderDetail == null)
            return NotFound();

        return Ok(orderDetail);
    }

    /// <summary>
    /// Rejects an order and initiates refund (tenant only).
    /// Changes order status to Rejected, initiates refund with payment gateway, and sends notification.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="request">Rejection details including reason.</param>
    /// <returns>Updated order details</returns>
    /// <response code="200">Order rejected and refund initiated.</response>
    /// <response code="400">Invalid order state for rejection.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden (not your store).</response>
    /// <response code="404">Order not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{orderId}/reject")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDetailDto>> RejectOrder(
        Guid orderId,
        [FromBody] RejectOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get tenant ID from claims
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var command = new RejectOrderCommand
        {
            OrderId = orderId,
            TenantId = tenantId,
            Reason = request.Reason
        };

        await _mediator.Send(command, cancellationToken);

        // Retrieve updated order details for response
        var query = new GetOrderByIdQuery
        {
            OrderId = orderId,
            TenantId = tenantId
        };

        var orderDetail = await _mediator.Send(query, cancellationToken);
        if (orderDetail == null)
            return NotFound();

        return Ok(orderDetail);
    }

    /// <summary>
    /// Adds shipment tracking information to an order (tenant only).
    /// Updates order with carrier, tracking number, and sends tracking email to customer.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="request">Shipment details including carrier and tracking number.</param>
    /// <returns>Updated order details</returns>
    /// <response code="200">Shipment tracking added.</response>
    /// <response code="400">Invalid shipment details.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden (not your store).</response>
    /// <response code="404">Order not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{orderId}/shipment")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDetailDto>> AddShipment(
        Guid orderId,
        [FromBody] AddShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get tenant ID from claims
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return Unauthorized();

        var command = new AddShipmentCommand
        {
            OrderId = orderId,
            TenantId = tenantId,
            Carrier = request.Carrier,
            TrackingNumber = request.TrackingNumber
        };

        await _mediator.Send(command, cancellationToken);

        // Retrieve updated order details for response
        var query = new GetOrderByIdQuery
        {
            OrderId = orderId,
            TenantId = tenantId
        };

        var orderDetail = await _mediator.Send(query, cancellationToken);
        if (orderDetail == null)
            return NotFound();

        return Ok(orderDetail);
    }

    /// <summary>
    /// Gets shipment tracking information for an order (customer/tenant).
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <returns>Shipment tracking details</returns>
    /// <response code="200">Returns tracking information.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden (not your order).</response>
    /// <response code="404">Order or tracking not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{orderId}/tracking")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShipmentTrackingDto>> GetTracking(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // Get current customer ID or tenant ID from claims
        var customerIdClaim = User.FindFirst("sub")?.Value;
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;

        Guid? customerId = null;
        Guid? tenantId = null;

        if (Guid.TryParse(customerIdClaim, out var custId))
            customerId = custId;

        if (Guid.TryParse(tenantIdClaim, out var tenId))
            tenantId = tenId;

        if ((!customerId.HasValue || customerId.Value == Guid.Empty) &&
            (!tenantId.HasValue || tenantId.Value == Guid.Empty))
            return Unauthorized();

        var query = new GetTrackingQuery
        {
            OrderId = orderId,
            CustomerId = customerId,
            TenantId = tenantId
        };

        var result = await _mediator.Send(query, cancellationToken);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Cancels a customer order (customer only).
    /// Can only cancel orders in Pending status. Initiates refund if already paid.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="request">Cancellation reason.</param>
    /// <returns>Updated order details</returns>
    /// <response code="200">Order cancelled.</response>
    /// <response code="400">Order cannot be cancelled in current state.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden (not your order).</response>
    /// <response code="404">Order not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{orderId}/cancel")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDetailDto>> CancelOrder(
        Guid orderId,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get current customer ID from claims (customer cancelling their own order)
        var customerIdClaim = User.FindFirst("sub")?.Value;
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;

        Guid? customerId = null;
        Guid? tenantId = null;

        if (Guid.TryParse(customerIdClaim, out var custId))
            customerId = custId;

        if (Guid.TryParse(tenantIdClaim, out var tenId))
            tenantId = tenId;

        if (!customerId.HasValue || customerId.Value == Guid.Empty)
            if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
                return Unauthorized();

        var command = new CancelOrderCommand
        {
            OrderId = orderId,
            CustomerId = customerId,
            TenantId = tenantId,
            Reason = request.Reason
        };

        await _mediator.Send(command, cancellationToken);

        // Retrieve updated order details for response
        var query = new GetOrderByIdQuery
        {
            OrderId = orderId,
            CustomerId = customerId,
            TenantId = tenantId
        };

        var orderDetail = await _mediator.Send(query, cancellationToken);
        if (orderDetail == null)
            return NotFound();

        return Ok(orderDetail);
    }

    /// <summary>
    /// Bulk updates status for multiple orders.
    /// Changes all specified orders to the new status in a single efficient operation.
    /// </summary>
    /// <param name="request">List of order IDs and new status.</param>
    /// <returns>Operation result with success/failure counts.</returns>
    /// <response code="200">Bulk update completed.</response>
    /// <response code="400">Validation error or invalid status.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("bulk-update-status")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> BulkUpdateOrderStatus(
        [FromBody] BulkUpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request?.OrderIds == null || !request.OrderIds.Any())
            return BadRequest(new { message = "No order IDs provided" });

        if (string.IsNullOrWhiteSpace(request.NewStatus))
            return BadRequest(new { message = "New status is required" });

        var command = new BulkUpdateOrderStatusCommand(request.OrderIds, request.NewStatus);
        var result = await _mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Exports orders to CSV file within a date range.
    /// Tenant admins can export their store orders for accounting and CRM integration.
    /// </summary>
    /// <param name="startDate">Start date for export filter (ISO 8601 format).</param>
    /// <param name="endDate">End date for export filter (ISO 8601 format).</param>
    /// <returns>CSV file with order data.</returns>
    /// <response code="200">Returns CSV file.</response>
    /// <response code="400">Invalid date range.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="403">Forbidden.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("export")]
    [Authorize(Roles = "TenantAdmin,StoreManager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExportOrders(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        if (endDate < startDate)
            return BadRequest(new { message = "End date must be after start date" });

        var query = new ExportOrdersQuery(startDate, endDate);
        var result = await _mediator.Send(query, cancellationToken);

        return File(result.CsvData, "text/csv", result.FileName);
    }
}

// DTOs - Bulk Operations
public sealed record BulkUpdateOrderStatusRequest(
    IEnumerable<Guid> OrderIds,
    string NewStatus
);

// DTOs - Order Management
public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    DateTime OrderDateUtc,
    decimal Total,
    string Status,
    int ItemCount);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    DateTime OrderDateUtc,
    Guid CustomerId,
    Guid TenantId,
    IReadOnlyList<OrderItemDto> Items,
    decimal SubTotal,
    decimal ShippingCost,
    decimal DiscountAmount,
    decimal Total,
    string Status,
    OrderAddressDto? ShippingAddress,
    OrderAddressDto? BillingAddress,
    ShipmentTrackingDto? Tracking);

public record OrderItemDto(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderAddressDto(
    string Name,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Phone);

public record ShipmentTrackingDto(
    string Carrier,
    string TrackingNumber,
    DateTime ShippedDateUtc,
    DateTime? DeliveredDateUtc,
    string Status);

public record RejectOrderRequest(string Reason);
public record CancelOrderRequest(string Reason);
public record AddShipmentRequest(string Carrier, string TrackingNumber);

