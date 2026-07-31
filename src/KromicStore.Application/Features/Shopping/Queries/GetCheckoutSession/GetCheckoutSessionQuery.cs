using MediatR;
using KromicStore.Application.Features.Shopping.Dtos;

namespace KromicStore.Application.Features.Shopping.Queries.GetCheckoutSession;

/// <summary>
/// Query to retrieve a checkout session with all details.
/// </summary>
public sealed record GetCheckoutSessionQuery(Guid CheckoutSessionId) : IRequest<GetCheckoutSessionResponse>;

/// <summary>
/// DTO for address details in the response.
/// </summary>
public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

/// <summary>
/// Response for GetCheckoutSession query.
/// </summary>
public sealed record GetCheckoutSessionResponse(
    Guid CheckoutSessionId,
    Guid TenantId,
    Guid CustomerId,
    string Currency,
    List<CheckoutItemDto> Items,
    int ItemsCount,
    decimal SubTotal,
    AddressDto? BillingAddress,
    AddressDto? ShippingAddress,
    string? ShippingMethodId,
    decimal ShippingCost,
    string? CouponCode,
    decimal DiscountAmount,
    decimal Total,
    string PaymentMethod,
    string PaymentStatus,
    string Status,
    DateTime CreatedOnUtc,
    DateTime LastModifiedOnUtc);
