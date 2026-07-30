using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.CreateProduct;

public sealed record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string Sku,
    string? CustomSlug = null,
    string? ShortDescription = null,
    string? Description = null,
    string? ProductType = "Physical",
    string? Status = "Draft",
    decimal Price = 0,
    decimal? CompareAtPrice = null,
    decimal? CostPrice = null,
    decimal? Weight = null,
    decimal? Length = null,
    decimal? Width = null,
    decimal? Height = null,
    bool IsFeatured = false,
    bool TrackInventory = true,
    bool Taxable = true,
    Dictionary<string, string>? Attributes = null,
    List<string>? Tags = null) : IRequest<CreateProductResponse>;

public sealed record CreateProductResponse(
    Guid ProductId,
    string Name,
    string Sku,
    string Slug);
