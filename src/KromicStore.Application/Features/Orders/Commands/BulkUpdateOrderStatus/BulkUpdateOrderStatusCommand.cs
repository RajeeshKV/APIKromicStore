using MediatR;

namespace KromicStore.Application.Features.Orders.Commands.BulkUpdateOrderStatus;

/// <summary>
/// Command to bulk update status of multiple orders.
/// Changes all specified orders to the new status in a single operation.
/// </summary>
public sealed record BulkUpdateOrderStatusCommand(
    IEnumerable<Guid> OrderIds,
    string NewStatus
) : IRequest<BulkUpdateOrderStatusResponse>;

/// <summary>
/// Response from bulk order status update with success/failure counts.
/// </summary>
public sealed record BulkUpdateOrderStatusResponse(
    int UpdatedCount,
    int FailedCount,
    List<BulkOrderOperationError> Errors
);

/// <summary>
/// Error details for individual orders that failed in bulk operation.
/// </summary>
public sealed record BulkOrderOperationError(
    Guid OrderId,
    string ErrorMessage
);
