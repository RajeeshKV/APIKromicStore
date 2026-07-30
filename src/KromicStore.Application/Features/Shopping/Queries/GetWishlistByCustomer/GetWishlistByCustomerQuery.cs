using MediatR;

namespace KromicStore.Application.Features.Shopping.Queries.GetWishlistByCustomer;

/// <summary>
/// Query to retrieve the wishlist for a customer.
/// </summary>
public sealed record GetWishlistByCustomerQuery(Guid CustomerId) : IRequest<GetWishlistByCustomerResponse>;

/// <summary>
/// DTO for a wishlist item in the response.
/// </summary>
public sealed record WishlistItemDto(
    Guid ProductId,
    Guid? VariantId,
    DateTime AddedOnUtc);

/// <summary>
/// Response for GetWishlistByCustomer query.
/// </summary>
public sealed record GetWishlistByCustomerResponse(
    Guid WishlistId,
    Guid CustomerId,
    List<WishlistItemDto> Items,
    int ItemsCount,
    DateTime CreatedOnUtc,
    DateTime LastModifiedOnUtc);
