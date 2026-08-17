using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ILogger<GetProductByIdQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetProductByIdResponse> Handle(
        GetProductByIdQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving product detail: {ProductId}", query.ProductId);

        // EF global query filter already scopes to the current tenant — a product
        // belonging to a different tenant simply won't be found (returns null), which
        // is the correct behaviour for both admin and storefront calls.
        var product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
        {
            _logger.LogWarning("Product not found or deleted: {ProductId}", query.ProductId);
            return new GetProductByIdResponse(null);
        }

        var productDto = MapToProductDetailDto(product);
        _logger.LogInformation("Product detail retrieved successfully: {ProductId}", query.ProductId);

        return new GetProductByIdResponse(productDto);
    }

    private static ProductDetailDto MapToProductDetailDto(dynamic product)
    {
        var variants = new List<VariantDto>();
        if (product.Variants != null && product.Variants.Count > 0)
        {
            foreach (var v in product.Variants)
            {
                var priceAdjustment = (decimal)(v.PriceAdjustment ?? 0);
                variants.Add(new VariantDto(
                    Id: v.Id,
                    Sku: v.Sku,
                    Name: v.Name,
                    Price: priceAdjustment > 0 ? product.Price + priceAdjustment : null,
                    CostPrice: null,
                    Attributes: v.Attributes != null ? BuildAttributesDictionary(v.Attributes) : new Dictionary<string, string>(),
                    QuantityOnHand: v.StockQuantity ?? 0,
                    IsAvailable: (v.IsActive ?? false) && (v.StockQuantity ?? 0) > 0,
                    CreatedAtUtc: DateTime.UtcNow));
            }
        }

        var images = new List<ProductImageDto>();
        if (product.Images != null && product.Images.Count > 0)
        {
            var imagesToAdd = new List<(dynamic image, int order)>();
            foreach (var i in product.Images)
            {
                if (!(i.IsDeleted ?? false))
                    imagesToAdd.Add((i, i.DisplayOrder ?? 0));
            }

            foreach (var (i, _) in imagesToAdd.OrderBy(x => x.order))
            {
                images.Add(new ProductImageDto(
                    Id: i.Id,
                    Url: i.Url,
                    AltText: i.AltText,
                    DisplayOrder: i.DisplayOrder ?? 0,
                    IsPrimary: i.IsPrimary ?? false,
                    CreatedAtUtc: DateTime.UtcNow));
            }
        }

        return new ProductDetailDto(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Sku: product.Sku,
            BasePrice: product.Price,
            CostPrice: product.CostPrice,
            CurrencyCode: "USD",
            IsAvailable: product.IsAvailable ?? false,
            QuantityOnHand: 0,
            ReorderLevel: 0,
            CategoryId: product.CategoryId,
            CategoryName: "",
            Variants: variants,
            Images: images,
            Attributes: BuildAttributesDictionary(product.Attributes),
            Tags: BuildTagsList(product.Tags),
            Slug: product.Slug,
            MetaDescription: product.MetaDescription ?? product.Description,
            CreatedAtUtc: product.CreatedOnUtc,
            ModifiedAtUtc: product.ModifiedOnUtc);
    }

    private static Dictionary<string, string> BuildAttributesDictionary(dynamic attributes)
    {
        var result = new Dictionary<string, string>();
        if (attributes != null)
        {
            foreach (var a in attributes)
                result[a.Name] = a.Value;
        }
        return result;
    }

    private static List<string> BuildTagsList(dynamic tags)
    {
        var result = new List<string>();
        if (tags != null)
        {
            foreach (var t in tags)
                result.Add(t.TagValue ?? string.Empty);
        }
        return result;
    }
}
