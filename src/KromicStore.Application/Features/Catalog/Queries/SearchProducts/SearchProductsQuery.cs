using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.SearchProducts;

/// <summary>
/// Query to search products by name, description, and tags.
/// </summary>
public sealed record SearchProductsQuery(
    string SearchText,
    int Skip = 0,
    int Take = 20,
    Guid? CategoryId = null) : IRequest<SearchProductsResponse>;

/// <summary>
/// Data transfer object for product search result.
/// </summary>
public sealed record ProductSearchResultDto(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal BasePrice,
    string CurrencyCode,
    string CategoryName,
    List<string> Tags,
    bool IsAvailable,
    float RelevanceScore);

/// <summary>
/// Response for SearchProducts query.
/// </summary>
public sealed record SearchProductsResponse(IEnumerable<ProductSearchResultDto> Data);
