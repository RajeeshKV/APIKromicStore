using MediatR;

namespace KromicStore.Application.Features.Catalog.Queries.GetVariants;

/// <summary>
/// Query to retrieve all variants for a specific product.
/// </summary>
public sealed record GetVariantsQuery(Guid ProductId) : IRequest<GetVariantsResponse>;

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
/// Response for GetVariants query.
/// </summary>
public sealed record GetVariantsResponse(IEnumerable<VariantDto> Data);
