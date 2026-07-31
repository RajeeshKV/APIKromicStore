using MediatR;

namespace KromicStore.Application.Features.Orders.Commands.ConfirmOrder;

/// <summary>
/// Confirm an order after successful payment.
/// Transitions order from Pending to Confirmed status.
/// </summary>
public sealed class ConfirmOrderCommand : IRequest<ConfirmOrderResponse>
{
    public Guid OrderId { get; set; }
    public Guid TenantId { get; set; }
}

public sealed class ConfirmOrderResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ConfirmedOnUtc { get; set; }
}
