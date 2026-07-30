using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetProducts;

/// <summary>
/// Query to retrieve all non-deleted products with optional filtering and pagination.
/// </summary>
public sealed record GetProductsQuery(
    int Skip = 0,
    int Take = 20,
    Guid? CategoryId = null,
    int? Status = null) : IRequest<GetProductsResponse>;

/// <summary>
/// Data transfer object for product card in query response.
/// </summary>
public sealed record ProductCardDto(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal BasePrice,
    string CurrencyCode,
    bool IsAvailable,
    int QuantityOnHand,
    Guid CategoryId,
    string CategoryName,
    List<string> Tags,
    DateTime CreatedAtUtc);

/// <summary>
/// Response for GetProducts query.
/// </summary>
public sealed record GetProductsResponse(IEnumerable<ProductCardDto> Data);
