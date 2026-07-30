using MediatR;

namespace KromicStore.Application.Features.Shopping.Queries.GetCartByCustomer;

/// <summary>
/// Query to retrieve the active shopping cart for a customer.
/// </summary>
public sealed record GetCartByCustomerQuery(Guid CustomerId) : IRequest<GetCartByCustomerResponse>;

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
/// Response for GetCartByCustomer query.
/// </summary>
public sealed record GetCartByCustomerResponse(
    Guid CartId,
    Guid CustomerId,
    string Currency,
    List<CartItemDto> Items,
    int ItemsCount,
    decimal SubTotal,
    DateTime LastActivityOnUtc,
    DateTime ExpiresOnUtc,
    bool IsExpired);
