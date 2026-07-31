using MediatR;

namespace KromicStore.Application.Features.Orders.Queries.GetTracking;

/// <summary>
/// Query to retrieve shipment tracking information for an order.
/// </summary>
public sealed class GetTrackingQuery : IRequest<ShipmentTrackingDto?>
{
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Optional: Customer ID for customer-portal access validation.
    /// </summary>
    public Guid? CustomerId { get; set; }
    
    /// <summary>
    /// Optional: Tenant ID for tenant-portal access validation.
    /// </summary>
    public Guid? TenantId { get; set; }
}

/// <summary>
/// DTO for shipment tracking information.
/// </summary>
public record ShipmentTrackingDto(
    string Carrier,
    string TrackingNumber,
    DateTime ShippedDateUtc,
    DateTime? DeliveredDateUtc,
    string Status);
