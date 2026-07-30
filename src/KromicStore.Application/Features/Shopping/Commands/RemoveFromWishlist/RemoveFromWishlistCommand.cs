using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.RemoveFromWishlist;

/// <summary>
/// Command to remove a product from a wishlist.
/// </summary>
public sealed record RemoveFromWishlistCommand(
    Guid WishlistId,
    Guid ProductId,
    Guid? VariantId = null) : IRequest<RemoveFromWishlistResponse>;

/// <summary>
/// Response for RemoveFromWishlist command.
/// </summary>
public sealed record RemoveFromWishlistResponse(
    Guid WishlistId,
    Guid ProductId,
    int ItemsCount,
    bool WasRemoved);
