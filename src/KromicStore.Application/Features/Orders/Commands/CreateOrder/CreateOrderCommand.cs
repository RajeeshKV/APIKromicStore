using MediatR;

namespace KromicStore.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Create a new order from a checkout session.
/// Transitions CheckoutSession from Draft/AwaitingPayment to Completed.
/// </summary>
public sealed class CreateOrderCommand : IRequest<CreateOrderResponse>
{
    public Guid CheckoutSessionId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid TenantId { get; set; }
}

public sealed class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}
