using MediatR;

namespace KromicStore.Application.Features.Shopping.Queries.GetCart;

/// <summary>
/// Query to retrieve a shopping cart by ID with all items.
/// </summary>
public sealed record GetCartQuery(Guid CartId) : IRequest<GetCartResponse>;

/// <summary>
/// DTO for a cart item in the response.
/// </summary>
public sealed record CartItemDto(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

/// <summary>
/// Response for GetCart query.
/// </summary>
public sealed record GetCartResponse(
    Guid CartId,
    Guid? CustomerId,
    string? AnonymousSessionId,
    string Currency,
    List<CartItemDto> Items,
    int ItemsCount,
    decimal SubTotal,
    DateTime LastActivityOnUtc,
    DateTime ExpiresOnUtc,
    bool IsExpired);
