using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetProductImages;

/// <summary>
/// Query to retrieve all images for a product ordered by display order.
/// </summary>
public sealed record GetProductImagesQuery(Guid ProductId) : IRequest<GetProductImagesResponse>;

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
/// Response for GetProductImages query.
/// </summary>
public sealed record GetProductImagesResponse(IEnumerable<ProductImageDto> Data);
