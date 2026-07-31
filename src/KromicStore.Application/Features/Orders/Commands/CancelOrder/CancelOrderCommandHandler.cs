using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Orders.Entities;
using KromicStore.Application.Features.Catalog.Commands.AdjustInventory;

namespace KromicStore.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRefundService _refundService;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IRefundService refundService,
        IMediator mediator,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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

        // Process refund if payment was captured
        string? refundReferenceId = null;
        if (order.PaymentId.HasValue && order.PaymentId.Value != Guid.Empty)
        {
            refundReferenceId = await ProcessRefundAsync(
                order.PaymentId.Value, 
                order.TenantId, 
                order.GrandTotal, 
                cancellationToken);
        }

        // Restore inventory from order items
        await RestoreInventoryAsync(order, cancellationToken);

        // Publish OrderCancelled domain event
        // TODO: Implement domain event publishing
        // This will trigger email notifications and other workflows
        _logger.LogInformation("Publishing OrderCancelled event for order {OrderNumber}", order.OrderNumber);

        // Update repository
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderNumber} cancelled successfully with refund {RefundReferenceId}", 
            order.OrderNumber, refundReferenceId ?? "N/A");

        return new CancelOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            RefundReferenceId = refundReferenceId,
            CancelledAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Processes refund through payment service if payment was captured.
    /// </summary>
    private async Task<string?> ProcessRefundAsync(
        Guid paymentId, 
        Guid tenantId, 
        decimal refundAmount, 
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found for refund processing", paymentId);
                return null;
            }

            // Only refund if payment was captured
            if (payment.Status != PaymentStatus.Completed)
            {
                _logger.LogInformation(
                    "Payment {PaymentId} is in {Status} status, skipping refund. Order will be cancelled.",
                    paymentId, payment.Status);
                return null;
            }

            if (string.IsNullOrWhiteSpace(payment.ProviderTransactionId))
            {
                _logger.LogWarning("Payment {PaymentId} has no provider transaction ID for refund", paymentId);
                return null;
            }

            // Call refund service to initiate refund
            _logger.LogInformation(
                "Initiating refund through payment service. PaymentId: {PaymentId}, Amount: {Amount}",
                paymentId, refundAmount);

            var refundId = await _refundService.RefundPaymentAsync(
                tenantId,
                payment.ProviderTransactionId,
                refundAmount,
                "Order cancelled by customer",
                cancellationToken);

            // Update payment status
            payment.ProcessRefund(refundAmount);
            _paymentRepository.Update(payment);

            _logger.LogInformation(
                "Refund processed successfully. PaymentId: {PaymentId}, RefundId: {RefundId}, Amount: {Amount}",
                paymentId, refundId, refundAmount);

            return refundId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during refund processing for payment {PaymentId}", paymentId);
            throw;
        }
    }

    /// <summary>
    /// Restores inventory for all items in the cancelled order.
    /// </summary>
    private async Task RestoreInventoryAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            if (order.Items == null || order.Items.Count == 0)
            {
                _logger.LogInformation("Order {OrderNumber} has no items to restore inventory for", order.OrderNumber);
                return;
            }

            foreach (var item in order.Items)
            {
                _logger.LogInformation(
                    "Restoring inventory for product {ProductId}. Quantity: {Quantity}",
                    item.ProductId, item.Quantity);

                // Use AdjustInventoryCommand to restore stock
                var adjustCommand = new AdjustInventoryCommand(
                    item.ProductId,
                    item.Quantity, // Positive to increase stock
                    $"Order cancellation: {order.OrderNumber}");

                try
                {
                    var result = await _mediator.Send(adjustCommand, cancellationToken);
                    _logger.LogInformation(
                        "Inventory restored for product {ProductId}. New quantity: {NewQuantity}",
                        item.ProductId, result.NewAvailableQuantity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to restore inventory for product {ProductId} from order {OrderNumber}",
                        item.ProductId, order.OrderNumber);
                    // Continue restoring other items even if one fails
                }
            }

            _logger.LogInformation("Inventory restoration completed for order {OrderNumber}", order.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during inventory restoration for order {OrderNumber}", order.OrderNumber);
            throw;
        }
    }
}
