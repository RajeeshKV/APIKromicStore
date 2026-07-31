using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Orders.Queries.GetTracking;

public sealed class GetTrackingQueryHandler : IRequestHandler<GetTrackingQuery, ShipmentTrackingDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetTrackingQueryHandler> _logger;

    public GetTrackingQueryHandler(
        IOrderRepository orderRepository,
        ILogger<GetTrackingQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ShipmentTrackingDto?> Handle(GetTrackingQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving tracking information for order {OrderId}", request.OrderId);

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
                    "Access denied: Customer {CustomerId} attempted to access tracking for order {OrderId} belonging to customer {OrderCustomerId}",
                    request.CustomerId.Value, request.OrderId, order.CustomerId);
                return null;
            }
        }

        if (request.TenantId.HasValue && request.TenantId.Value != Guid.Empty)
        {
            if (order.TenantId != request.TenantId.Value)
            {
                _logger.LogWarning(
                    "Access denied: Tenant {TenantId} attempted to access tracking for order {OrderId} belonging to tenant {OrderTenantId}",
                    request.TenantId.Value, request.OrderId, order.TenantId);
                return null;
            }
        }

        // If order hasn't been shipped yet, return null (no tracking available)
        if (!order.ShippedOnUtc.HasValue)
        {
            _logger.LogInformation("Order {OrderId} has not been shipped yet", request.OrderId);
            return null;
        }

        _logger.LogInformation("Retrieved tracking for order {OrderNumber} successfully", order.OrderNumber);

        // Return shipment tracking DTO
        // Extract tracking number from timeline if available (stored during shipment marking)
        var trackingNumber = ExtractTrackingNumberFromTimeline(order);

        return new ShipmentTrackingDto(
            Carrier: order.ShippingMethod ?? string.Empty,
            TrackingNumber: trackingNumber,
            ShippedDateUtc: order.ShippedOnUtc.Value,
            DeliveredDateUtc: order.DeliveredOnUtc,
            Status: order.Status.ToString());
    }

    /// <summary>
    /// Extract tracking number from order timeline.
    /// Timeline entries may contain tracking info like "Order shipped. Tracking: ABC123"
    /// </summary>
    private static string ExtractTrackingNumberFromTimeline(Domain.Orders.Entities.Order order)
    {
        var shippedEntry = order.Timeline
            .Where(t => t.EventDescription.Contains("shipped", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(t => t.CreatedOnUtc)
            .FirstOrDefault();

        if (shippedEntry?.EventDescription != null)
        {
            const string trackingPrefix = "Tracking: ";
            var trackingIndex = shippedEntry.EventDescription.IndexOf(trackingPrefix, StringComparison.OrdinalIgnoreCase);
            if (trackingIndex >= 0)
            {
                var startIndex = trackingIndex + trackingPrefix.Length;
                var trackingPart = shippedEntry.EventDescription.Substring(startIndex).Trim();
                return trackingPart;
            }
        }

        return string.Empty;
    }
}
