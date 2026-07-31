using MediatR;

namespace KromicStore.Application.Features.Orders.Queries.GetOrderById;

public record OrderItemDto(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderAddressDto(
    string Name,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Phone);

public record ShipmentTrackingDto(
    string Carrier,
    string TrackingNumber,
    DateTime ShippedDateUtc,
    DateTime? DeliveredDateUtc,
    string Status);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    DateTime OrderDateUtc,
    Guid CustomerId,
    Guid TenantId,
    IReadOnlyList<OrderItemDto> Items,
    decimal SubTotal,
    decimal ShippingCost,
    decimal DiscountAmount,
    decimal Total,
    string Status,
    OrderAddressDto? ShippingAddress,
    OrderAddressDto? BillingAddress,
    ShipmentTrackingDto? Tracking);

/// <summary>
/// Query to retrieve a specific order by ID.
/// </summary>
public sealed class GetOrderByIdQuery : IRequest<OrderDetailDto?>
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
