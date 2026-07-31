using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Orders.Commands.AddShipment;

public sealed class AddShipmentCommandHandler : IRequestHandler<AddShipmentCommand, AddShipmentResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AddShipmentCommandHandler> _logger;

    public AddShipmentCommandHandler(
        IOrderRepository orderRepository,
        ILogger<AddShipmentCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AddShipmentResponse> Handle(AddShipmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding shipment to order {OrderId}: Carrier={Carrier}, TrackingNumber={TrackingNumber}",
            request.OrderId, request.Carrier, request.TrackingNumber);

        // Retrieve order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order {request.OrderId} not found");

        // Verify tenant ownership
        if (order.TenantId != request.TenantId)
            throw new UnauthorizedAccessException($"Tenant {request.TenantId} does not own order {request.OrderId}");

        // Validate order can be shipped (must be confirmed)
        if (order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException(
                $"Cannot ship order in {order.Status} status. Only confirmed orders can be shipped.");

        // Mark as shipped
        order.MarkAsShipped(request.TrackingNumber);

        // TODO: Store carrier and tracking number in a Fulfillment entity or add to order
        // TODO: Publish OrderShipped domain event to trigger customer notification with tracking info

        // Update repository
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order {OrderNumber} marked as shipped with tracking {TrackingNumber}",
            order.OrderNumber, request.TrackingNumber);

        return new AddShipmentResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            Carrier = request.Carrier,
            TrackingNumber = request.TrackingNumber,
            ShippedAtUtc = order.ShippedOnUtc ?? DateTime.UtcNow
        };
    }
}
