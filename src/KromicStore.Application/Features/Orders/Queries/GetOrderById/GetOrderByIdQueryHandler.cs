using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderDetailDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving order {OrderId}", request.OrderId);

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            return null;
        }

        // Validate access: customer can only see their own orders, tenant admin can see store orders
        if (request.CustomerId.HasValue && request.CustomerId.Value != Guid.Empty)
        {
            if (order.CustomerId != request.CustomerId.Value)
            {
                _logger.LogWarning(
                    "Access denied: Customer {CustomerId} attempted to access order {OrderId} belonging to customer {OrderCustomerId}",
                    request.CustomerId.Value, request.OrderId, order.CustomerId);
                return null;
            }
        }

        if (request.TenantId.HasValue && request.TenantId.Value != Guid.Empty)
        {
            if (order.TenantId != request.TenantId.Value)
            {
                _logger.LogWarning(
                    "Access denied: Tenant {TenantId} attempted to access order {OrderId} belonging to tenant {OrderTenantId}",
                    request.TenantId.Value, request.OrderId, order.TenantId);
                return null;
            }
        }

        _logger.LogInformation("Retrieved order {OrderNumber} successfully", order.OrderNumber);

        // Map order to DTO
        return MapToDetailDto(order);
    }

    private OrderDetailDto MapToDetailDto(Domain.Orders.Entities.Order order)
    {
        var items = order.Items
            .Select(item => new OrderItemDto(
                ProductId: item.ProductId,
                VariantId: item.ProductVariantId,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                LineTotal: item.LineTotal))
            .ToList()
            .AsReadOnly();

        // Note: Address details would come from a separate address repository lookup
        // For now, we return null for addresses, but this should be completed when address service is integrated
        OrderAddressDto? shippingAddress = null;
        OrderAddressDto? billingAddress = null;

        // TODO: Load actual address details from AddressRepository using BillingAddressId and ShippingAddressId

        ShipmentTrackingDto? tracking = null;
        if (order.ShippedOnUtc.HasValue)
        {
            tracking = new ShipmentTrackingDto(
                Carrier: order.ShippingMethod,
                TrackingNumber: "", // TODO: Load from Fulfillment entity when available
                ShippedDateUtc: order.ShippedOnUtc.Value,
                DeliveredDateUtc: order.DeliveredOnUtc,
                Status: order.Status.ToString());
        }

        return new OrderDetailDto(
            Id: order.Id,
            OrderNumber: order.OrderNumber,
            OrderDateUtc: order.CreatedOnUtc,
            CustomerId: order.CustomerId,
            TenantId: order.TenantId,
            Items: items,
            SubTotal: order.SubTotal,
            ShippingCost: order.ShippingAmount,
            DiscountAmount: order.DiscountAmount,
            Total: order.GrandTotal,
            Status: order.Status.ToString(),
            ShippingAddress: shippingAddress,
            BillingAddress: billingAddress,
            Tracking: tracking);
    }
}
