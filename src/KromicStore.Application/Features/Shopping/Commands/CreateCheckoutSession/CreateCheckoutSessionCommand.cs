using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.CreateCheckoutSession;

/// <summary>
/// Command to create a new checkout session from a cart.
/// </summary>
public sealed record CreateCheckoutSessionCommand(
    Guid CartId,
    Guid CustomerId) : IRequest<CreateCheckoutSessionResponse>;

/// <summary>
/// DTO for a checkout item in the response.
/// </summary>
public sealed record CheckoutItemDto(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>
/// Response for CreateCheckoutSession command.
/// </summary>
public sealed record CreateCheckoutSessionResponse(
    Guid CheckoutSessionId,
    Guid CustomerId,
    string Currency,
    List<CheckoutItemDto> Items,
    int ItemsCount,
    decimal SubTotal,
    string Status,
    DateTime CreatedOnUtc);
