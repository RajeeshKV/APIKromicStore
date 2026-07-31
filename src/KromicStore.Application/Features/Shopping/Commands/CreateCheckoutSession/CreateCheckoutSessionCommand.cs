using MediatR;
using KromicStore.Application.Features.Shopping.Dtos;

namespace KromicStore.Application.Features.Shopping.Commands.CreateCheckoutSession;

/// <summary>
/// Command to create a new checkout session from a cart.
/// </summary>
public sealed record CreateCheckoutSessionCommand(
    Guid CartId,
    Guid CustomerId) : IRequest<CreateCheckoutSessionResponse>;

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
