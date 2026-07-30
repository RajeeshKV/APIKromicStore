using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveCartItem;

/// <summary>
/// Command to remove a specific item from a shopping cart.
/// </summary>
public sealed record RemoveCartItemCommand(
    Guid CartId,
    Guid ProductId,
    Guid? VariantId = null) : IRequest<RemoveCartItemResponse>;

/// <summary>
/// Response for RemoveCartItem command.
/// </summary>
public sealed record RemoveCartItemResponse(
    Guid CartId,
    Guid ProductId,
    Guid? VariantId,
    bool ItemFound,
    int CartItemsCount,
    decimal CartSubTotal);
