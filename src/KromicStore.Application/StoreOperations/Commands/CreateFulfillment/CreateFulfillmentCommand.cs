using MediatR;

namespace KromicStore.Application.StoreOperations.Commands.CreateFulfillment;

/// <summary>
/// Command to create a new fulfillment for an order.
/// </summary>
public sealed class CreateFulfillmentCommand : IRequest<CreateFulfillmentResponse>
{
    public Guid OrderId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
}

public sealed class CreateFulfillmentResponse
{
    public Guid FulfillmentId { get; set; }
    public Guid OrderId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
}
