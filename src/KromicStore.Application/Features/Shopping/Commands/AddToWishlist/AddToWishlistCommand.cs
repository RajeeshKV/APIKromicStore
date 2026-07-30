using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.AddToWishlist;

/// <summary>
/// Command to add a product to a wishlist.
/// </summary>
public sealed record AddToWishlistCommand(
    Guid WishlistId,
    Guid ProductId,
    Guid? VariantId = null) : IRequest<AddToWishlistResponse>;

/// <summary>
/// Response for AddToWishlist command.
/// </summary>
public sealed record AddToWishlistResponse(
    Guid WishlistId,
    Guid ProductId,
    int ItemsCount,
    bool IsNew);
