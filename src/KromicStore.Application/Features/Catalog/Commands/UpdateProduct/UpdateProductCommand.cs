using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    Guid? CategoryId = null,
    string? Name = null,
    string? Sku = null,
    string? CustomSlug = null,
    string? ShortDescription = null,
    string? Description = null,
    string? Status = null,
    decimal? Price = null,
    decimal? CompareAtPrice = null,
    decimal? CostPrice = null,
    decimal? Weight = null,
    decimal? Length = null,
    decimal? Width = null,
    decimal? Height = null,
    bool? IsFeatured = null,
    bool? Taxable = null) : IRequest<UpdateProductResponse>;

public sealed record UpdateProductResponse(
    Guid ProductId,
    string Name,
    string Sku,
    string Slug);
