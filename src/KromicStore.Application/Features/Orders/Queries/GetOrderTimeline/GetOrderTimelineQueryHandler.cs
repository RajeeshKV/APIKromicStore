using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Orders.Queries.GetOrderTimeline;

public sealed class GetOrderTimelineQueryHandler : IRequestHandler<GetOrderTimelineQuery, GetOrderTimelineResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrderTimelineQueryHandler> _logger;

    public GetOrderTimelineQueryHandler(
        IOrderRepository orderRepository,
        ILogger<GetOrderTimelineQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetOrderTimelineResponse> Handle(GetOrderTimelineQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving timeline for order {OrderId}", request.OrderId);

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            return new GetOrderTimelineResponse { OrderId = request.OrderId };
        }

        // Validate access: customer can only see their own orders, tenant admin can see store orders
        if (request.CustomerId.HasValue && request.CustomerId.Value != Guid.Empty)
        {
            if (order.CustomerId != request.CustomerId.Value)
            {
                _logger.LogWarning(
                    "Access denied: Customer {CustomerId} attempted to access timeline for order {OrderId}",
                    request.CustomerId.Value, request.OrderId);
                return new GetOrderTimelineResponse { OrderId = request.OrderId };
            }
        }

        if (request.TenantId.HasValue && request.TenantId.Value != Guid.Empty)
        {
            if (order.TenantId != request.TenantId.Value)
            {
                _logger.LogWarning(
                    "Access denied: Tenant {TenantId} attempted to access timeline for order {OrderId}",
                    request.TenantId.Value, request.OrderId);
                return new GetOrderTimelineResponse { OrderId = request.OrderId };
            }
        }

        var events = order.Timeline
            .OrderBy(t => t.CreatedOnUtc)
            .Select(t => new OrderTimelineEventDto
            {
                EventId = t.Id,
                EventTitle = MapStatusToTitle(order.Status, t.EventDescription),
                EventDescription = t.EventDescription,
                EventType = DetermineEventType(t.EventDescription),
                OccurredAtUtc = t.CreatedOnUtc,
                Notes = null
            })
            .ToList();

        _logger.LogInformation("Retrieved {EventCount} timeline events for order {OrderNumber}", events.Count, order.OrderNumber);

        return new GetOrderTimelineResponse
        {
            OrderId = order.Id,
            Events = events
        };
    }

    private string MapStatusToTitle(Domain.Orders.Entities.OrderStatus status, string eventDescription)
    {
        return status switch
        {
            Domain.Orders.Entities.OrderStatus.Pending => "Order Placed",
            Domain.Orders.Entities.OrderStatus.Confirmed => "Order Confirmed",
            Domain.Orders.Entities.OrderStatus.Shipped => "Shipped",
            Domain.Orders.Entities.OrderStatus.Delivered => "Delivered",
            Domain.Orders.Entities.OrderStatus.Cancelled => "Cancelled",
            Domain.Orders.Entities.OrderStatus.PartiallyReturned => "Partially Returned",
            Domain.Orders.Entities.OrderStatus.Returned => "Returned",
            _ => "Order Update"
        };
    }

    private string DetermineEventType(string eventDescription)
    {
        if (eventDescription.Contains("created", StringComparison.OrdinalIgnoreCase))
            return "Created";

        if (eventDescription.Contains("confirmed", StringComparison.OrdinalIgnoreCase))
            return "Confirmed";

        if (eventDescription.Contains("shipped", StringComparison.OrdinalIgnoreCase) ||
            eventDescription.Contains("tracking", StringComparison.OrdinalIgnoreCase))
            return "Shipped";

        if (eventDescription.Contains("delivered", StringComparison.OrdinalIgnoreCase))
            return "Delivered";

        if (eventDescription.Contains("cancelled", StringComparison.OrdinalIgnoreCase))
            return "Cancelled";

        if (eventDescription.Contains("refund", StringComparison.OrdinalIgnoreCase))
            return "Refunded";

        if (eventDescription.Contains("return", StringComparison.OrdinalIgnoreCase))
            return "Return";

        return "Update";
    }
}
