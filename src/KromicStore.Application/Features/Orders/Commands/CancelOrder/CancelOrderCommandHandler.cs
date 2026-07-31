using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CancelOrderResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling order {OrderId}", request.OrderId);

        // Retrieve order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order {request.OrderId} not found");

        // Validate access
        if (request.CustomerId.HasValue && request.CustomerId.Value != Guid.Empty)
        {
            if (order.CustomerId != request.CustomerId.Value)
                throw new UnauthorizedAccessException(
                    $"Customer {request.CustomerId.Value} does not own order {request.OrderId}");
        }

        if (request.TenantId.HasValue && request.TenantId.Value != Guid.Empty)
        {
            if (order.TenantId != request.TenantId.Value)
                throw new UnauthorizedAccessException(
                    $"Tenant {request.TenantId.Value} does not own order {request.OrderId}");
        }

        // Validate order can be cancelled
        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException(
                $"Cannot cancel order in {order.Status} status.");

        // Cancel the order
        order.Cancel(request.Reason);

        // TODO: Trigger refund if payment was captured
        string? refundReferenceId = null;
        if (order.PaymentId.HasValue && order.PaymentId.Value != Guid.Empty)
        {
            var payment = await _paymentRepository.GetByIdAsync(order.PaymentId.Value, cancellationToken);
            if (payment != null && payment.Status == PaymentStatus.Completed)
            {
                // TODO: Call payment gateway to initiate refund
                _logger.LogWarning(
                    "Order {OrderNumber} requires refund processing for payment {PaymentId}",
                    order.OrderNumber, order.PaymentId);

                // TODO: Set refundReferenceId from refund gateway response
            }
        }

        // TODO: Restore inventory from order items

        // TODO: Publish OrderCancelled domain event
        // This will trigger email notifications and other workflows

        // Update repository
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} cancelled successfully", order.OrderNumber);

        return new CancelOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            RefundReferenceId = refundReferenceId,
            CancelledAtUtc = DateTime.UtcNow
        };
    }
}
