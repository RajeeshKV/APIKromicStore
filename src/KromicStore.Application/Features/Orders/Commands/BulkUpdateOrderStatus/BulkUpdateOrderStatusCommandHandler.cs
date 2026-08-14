using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Orders.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Orders.Commands.BulkUpdateOrderStatus;

/// <summary>
/// Handles bulk status updates for orders.
/// Validates each order, updates status, and returns success/failure counts.
/// </summary>
public sealed class BulkUpdateOrderStatusCommandHandler
    : IRequestHandler<BulkUpdateOrderStatusCommand, BulkUpdateOrderStatusResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<BulkUpdateOrderStatusCommandHandler> _logger;

    public BulkUpdateOrderStatusCommandHandler(
        IApplicationDbContext db,
        ILogger<BulkUpdateOrderStatusCommandHandler> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BulkUpdateOrderStatusResponse> Handle(
        BulkUpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<BulkOrderOperationError>();
        int updatedCount = 0;
        var orderIdsList = request.OrderIds.ToList();

        if (!orderIdsList.Any())
        {
            _logger.LogWarning("Bulk update attempted with empty order list");
            throw new InvalidOperationException("At least one order ID is required");
        }

        // Validate new status
        if (!Enum.TryParse<OrderStatus>(request.NewStatus, true, out var newStatus))
        {
            var validStatuses = string.Join(", ", Enum.GetNames(typeof(OrderStatus)));
            throw new InvalidOperationException($"Invalid status. Valid statuses: {validStatuses}");
        }

        foreach (var orderId in orderIdsList)
        {
            try
            {
                if (orderId == Guid.Empty)
                {
                    errors.Add(new BulkOrderOperationError(orderId, "Invalid order ID format"));
                    continue;
                }

                var order = await _db.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

                if (order == null)
                {
                    errors.Add(new BulkOrderOperationError(orderId, "Order not found"));
                    continue;
                }

                // Validate status transition
                if (!CanTransitionTo(order.Status, newStatus))
                {
                    errors.Add(new BulkOrderOperationError(
                        orderId,
                        $"Cannot transition from {order.Status} to {newStatus}"));
                    continue;
                }

                // Update status - use appropriate method on Order entity
                if (newStatus == OrderStatus.Confirmed)
                    order.Confirm();
                else if (newStatus == OrderStatus.Shipped)
                    order.MarkAsShipped();
                else if (newStatus == OrderStatus.Delivered)
                    order.MarkAsDelivered();
                else if (newStatus == OrderStatus.Cancelled)
                    order.Cancel();

                updatedCount++;

                _logger.LogInformation("Order status updated: {OrderId} -> {NewStatus}", orderId, newStatus);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update order {OrderId}", orderId);
                errors.Add(new BulkOrderOperationError(orderId, ex.Message));
            }
        }

        // Single SaveChanges for all operations (batch efficiency)
        if (updatedCount > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Bulk status update completed: {UpdatedCount} orders updated, {FailedCount} failed",
                updatedCount, errors.Count);
        }

        return new BulkUpdateOrderStatusResponse(
            UpdatedCount: updatedCount,
            FailedCount: errors.Count,
            Errors: errors
        );
    }

    /// <summary>
    /// Validates if status can transition from current to target.
    /// </summary>
    private static bool CanTransitionTo(OrderStatus currentStatus, OrderStatus targetStatus)
    {
        // Valid transitions
        return (currentStatus, targetStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            (OrderStatus.Shipped, OrderStatus.Cancelled) => true,
            (_, OrderStatus.Cancelled) => true, // Can cancel from most states
            (var current, var target) when current == target => true, // Same status (no-op)
            _ => false
        };
    }
}
