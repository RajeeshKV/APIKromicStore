using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.PlaceOrder;

/// <summary>
/// Command to place an order from a checkout session.
/// </summary>
public sealed record PlaceOrderCommand(
    Guid CheckoutSessionId,
    string PaymentTransactionId) : IRequest<PlaceOrderResponse>;

/// <summary>
/// DTO for an order item in the response.
/// </summary>
public sealed record OrderItemDto(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>
/// Response for PlaceOrder command.
/// </summary>
public sealed record PlaceOrderResponse(
    Guid OrderId,
    Guid CustomerId,
    string OrderNumber,
    List<OrderItemDto> Items,
    decimal SubTotal,
    decimal ShippingCost,
    decimal DiscountAmount,
    decimal Total,
    string Status,
    DateTime CreatedOnUtc);
