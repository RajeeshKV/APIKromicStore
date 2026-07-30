using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.UpdateCartItem;

/// <summary>
/// Command to update the quantity of a cart item.
/// Setting quantity to 0 removes the item.
/// </summary>
public sealed record UpdateCartItemCommand(
    Guid CartId,
    Guid ProductId,
    int NewQuantity,
    Guid? VariantId = null) : IRequest<UpdateCartItemResponse>;

/// <summary>
/// Response for UpdateCartItem command.
/// </summary>
public sealed record UpdateCartItemResponse(
    Guid CartId,
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    int CartItemsCount,
    decimal CartSubTotal,
    bool ItemRemoved);
