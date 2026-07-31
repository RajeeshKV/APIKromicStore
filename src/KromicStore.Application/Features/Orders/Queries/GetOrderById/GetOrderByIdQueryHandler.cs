using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Application.Common.Repositories;

namespace KromicStore.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerAddressRepository _addressRepository;
    private readonly IFulfillmentRepository _fulfillmentRepository;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        ICustomerAddressRepository addressRepository,
        IFulfillmentRepository fulfillmentRepository,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _fulfillmentRepository = fulfillmentRepository ?? throw new ArgumentNullException(nameof(fulfillmentRepository));
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

        // Map order to DTO with full address and tracking details
        return await MapToDetailDtoAsync(order, cancellationToken);
    }

    private async Task<OrderDetailDto> MapToDetailDtoAsync(
        Domain.Orders.Entities.Order order,
        CancellationToken cancellationToken = default)
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

        // Load actual address details from AddressRepository
        OrderAddressDto? shippingAddress = null;
        OrderAddressDto? billingAddress = null;

        if (order.ShippingAddressId != Guid.Empty)
        {
            var shippingAddr = await _addressRepository.GetByIdAsync(order.ShippingAddressId, cancellationToken);
            if (shippingAddr != null)
            {
                shippingAddress = new OrderAddressDto(
                    Name: shippingAddr.Label,
                    Street: shippingAddr.Street,
                    City: shippingAddr.City,
                    State: shippingAddr.StateCode,
                    PostalCode: shippingAddr.PostalCode,
                    Country: shippingAddr.CountryCode,
                    Phone: shippingAddr.PhoneNumber ?? string.Empty);
            }
        }

        if (order.BillingAddressId != Guid.Empty)
        {
            var billingAddr = await _addressRepository.GetByIdAsync(order.BillingAddressId, cancellationToken);
            if (billingAddr != null)
            {
                billingAddress = new OrderAddressDto(
                    Name: billingAddr.Label,
                    Street: billingAddr.Street,
                    City: billingAddr.City,
                    State: billingAddr.StateCode,
                    PostalCode: billingAddr.PostalCode,
                    Country: billingAddr.CountryCode,
                    Phone: billingAddr.PhoneNumber ?? string.Empty);
            }
        }

        // Load tracking information from Fulfillment entity
        ShipmentTrackingDto? tracking = null;
        if (order.ShippedOnUtc.HasValue)
        {
            var fulfillment = await _fulfillmentRepository.GetByOrderIdAsync(order.Id, cancellationToken);
            var trackingNumber = fulfillment?.TrackingNumber ?? "";
            
            tracking = new ShipmentTrackingDto(
                Carrier: order.ShippingMethod,
                TrackingNumber: trackingNumber,
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
