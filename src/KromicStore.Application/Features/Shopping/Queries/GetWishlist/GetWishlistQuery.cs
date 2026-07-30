using MediatR;

namespace KromicStore.Application.Features.Shopping.Queries.GetWishlist;

/// <summary>
/// Query to retrieve a wishlist by ID with all items.
/// </summary>
public sealed record GetWishlistQuery(Guid WishlistId) : IRequest<GetWishlistResponse>;

/// <summary>
/// DTO for a wishlist item in the response.
/// </summary>
public sealed record WishlistItemDto(
    Guid ProductId,
    Guid? VariantId,
    DateTime AddedOnUtc);

/// <summary>
/// Response for GetWishlist query.
/// </summary>
public sealed record GetWishlistResponse(
    Guid WishlistId,
    Guid CustomerId,
    List<WishlistItemDto> Items,
    int ItemsCount,
    DateTime CreatedOnUtc,
    DateTime LastModifiedOnUtc);
