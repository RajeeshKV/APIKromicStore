using MediatR;

namespace KromicStore.Application.Features.Orders.Commands.RejectOrder;

/// <summary>
/// Command to reject a pending order (typically by tenant admin).
/// Initiates refund if payment was captured and sends notification to customer.
/// </summary>
public sealed class RejectOrderCommand : IRequest<RejectOrderResponse>
{
    public Guid OrderId { get; set; }
    public Guid TenantId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class RejectOrderResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RefundReferenceId { get; set; }
    public DateTime RejectedAtUtc { get; set; }
}
