using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.AddToCart;

/// <summary>
/// Command to add an item to a shopping cart.
/// Creates the cart item if it doesn't exist, or increases quantity if it does.
/// </summary>
public sealed record AddToCartCommand(
    Guid CartId,
    Guid ProductId,
    decimal UnitPrice,
    int Quantity = 1,
    Guid? VariantId = null) : IRequest<AddToCartResponse>;

/// <summary>
/// Response for AddToCart command.
/// </summary>
public sealed record AddToCartResponse(
    Guid CartId,
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    int CartItemsCount,
    decimal CartSubTotal);
