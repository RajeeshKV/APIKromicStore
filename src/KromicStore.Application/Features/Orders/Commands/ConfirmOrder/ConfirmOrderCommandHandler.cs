using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Orders.Commands.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, ConfirmOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ConfirmOrderCommandHandler> _logger;

    public ConfirmOrderCommandHandler(
        IOrderRepository orderRepository,
        ILogger<ConfirmOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConfirmOrderResponse> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        // Retrieve order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order {request.OrderId} not found");

        // Validate order can be confirmed
        if (order.Status != Domain.Orders.Entities.OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order in {order.Status} status. Expected: Pending");

        // Confirm order
        order.Confirm();

        // Update repository
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} confirmed", order.OrderNumber);

        return new ConfirmOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            ConfirmedOnUtc = DateTime.UtcNow
        };
    }
}
