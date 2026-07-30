using MediatR;

namespace KromicStore.Application.Features.Shopping.Commands.CreateWishlist;

/// <summary>
/// Command to create a new wishlist for a customer.
/// </summary>
public sealed record CreateWishlistCommand(Guid CustomerId) : IRequest<CreateWishlistResponse>;

/// <summary>
/// Response for CreateWishlist command.
/// </summary>
public sealed record CreateWishlistResponse(
    Guid WishlistId,
    Guid CustomerId,
    int ItemsCount);
