using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Orders.Commands.RejectOrder;

public sealed class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand, RejectOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<RejectOrderCommandHandler> _logger;

    public RejectOrderCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        ILogger<RejectOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RejectOrderResponse> Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting order {OrderId} for tenant {TenantId}", request.OrderId, request.TenantId);

        // Retrieve order
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order {request.OrderId} not found");

        // Verify tenant ownership
        if (order.TenantId != request.TenantId)
            throw new UnauthorizedAccessException($"Tenant {request.TenantId} does not own order {request.OrderId}");

        // Validate order can be rejected (only pending orders can be rejected)
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot reject order in {order.Status} status. Only pending orders can be rejected.");

        // Cancel the order with rejection reason
        order.Cancel($"Rejected by store: {request.Reason}");

        // TODO: Trigger refund if payment was captured
        // Check if order has payment
        string? refundReferenceId = null;
        if (order.PaymentId.HasValue && order.PaymentId.Value != Guid.Empty)
        {
            var payment = await _paymentRepository.GetByIdAsync(order.PaymentId.Value, cancellationToken);
            if (payment != null && payment.Status == PaymentStatus.Completed)
            {
                // TODO: Call payment gateway to initiate refund
                // For now, just log the intention
                _logger.LogWarning(
                    "Order {OrderNumber} requires refund processing for payment {PaymentId}",
                    order.OrderNumber, order.PaymentId);

                // TODO: Set refundReferenceId from refund gateway response
            }
        }

        // TODO: Publish OrderRejected domain event
        // This will trigger email notifications and inventory restoration

        // Update repository
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} rejected successfully", order.OrderNumber);

        return new RejectOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            RefundReferenceId = refundReferenceId,
            RejectedAtUtc = DateTime.UtcNow
        };
    }
}
