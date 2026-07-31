using MediatR;

namespace KromicStore.Application.Features.Storefront.Queries.ListFeaturedProducts;

/// <summary>
/// Query to retrieve featured products from collections
/// </summary>
public record ListFeaturedProductsQuery(int Take = 12) : IRequest<ListFeaturedProductsResponse>;

public record ListFeaturedProductsResponse(IReadOnlyList<FeaturedProductDto> Products);

public record FeaturedProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal BasePrice,
    decimal? DiscountedPrice,
    string? ImageUrl,
    string? Sku,
    bool IsAvailable,
    int QuantityOnHand,
    string? CollectionName);
