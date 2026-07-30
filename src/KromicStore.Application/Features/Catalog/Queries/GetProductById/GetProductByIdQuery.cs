using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetProductById;

/// <summary>
/// Query to retrieve a single product by ID with all details.
/// </summary>
public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<GetProductByIdResponse>;

/// <summary>
/// Data transfer object for product image in query response.
/// </summary>
public sealed record ProductImageDto(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary,
    DateTime CreatedAtUtc);

/// <summary>
/// Data transfer object for product variant in query response.
/// </summary>
public sealed record VariantDto(
    Guid Id,
    string Sku,
    string? Name,
    decimal? Price,
    decimal? CostPrice,
    Dictionary<string, string> Attributes,
    int QuantityOnHand,
    bool IsAvailable,
    DateTime CreatedAtUtc);

/// <summary>
/// Data transfer object for complete product information.
/// </summary>
public sealed record ProductDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal BasePrice,
    decimal? CostPrice,
    string CurrencyCode,
    bool IsAvailable,
    int QuantityOnHand,
    int ReorderLevel,
    Guid CategoryId,
    string CategoryName,
    List<VariantDto> Variants,
    List<ProductImageDto> Images,
    Dictionary<string, string> Attributes,
    List<string> Tags,
    string Slug,
    string? MetaDescription,
    DateTime CreatedAtUtc,
    DateTime? ModifiedAtUtc);

/// <summary>
/// Response for GetProductById query.
/// </summary>
public sealed record GetProductByIdResponse(ProductDetailDto? Data);
