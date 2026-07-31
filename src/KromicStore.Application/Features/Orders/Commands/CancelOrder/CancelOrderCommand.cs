using MediatR;

namespace KromicStore.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Command to cancel an order (customer or tenant initiated).
/// Initiates refund if applicable and restores inventory.
/// </summary>
public sealed class CancelOrderCommand : IRequest<CancelOrderResponse>
{
    public Guid OrderId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? TenantId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class CancelOrderResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RefundReferenceId { get; set; }
    public DateTime CancelledAtUtc { get; set; }
}
