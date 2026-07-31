using MediatR;

namespace KromicStore.Application.Features.Orders.Commands.AddShipment;

/// <summary>
/// Command to add shipment tracking information to an order.
/// Typically called by tenant admin when shipping the order.
/// Sends tracking notification to customer.
/// </summary>
public sealed class AddShipmentCommand : IRequest<AddShipmentResponse>
{
    public Guid OrderId { get; set; }
    public Guid TenantId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
}

public sealed class AddShipmentResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime ShippedAtUtc { get; set; }
}
