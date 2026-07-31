using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        ILogger<GetProductByIdQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetProductByIdResponse> Handle(
        GetProductByIdQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving product detail: {ProductId}", query.ProductId);

        var product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
        {
            _logger.LogWarning("Product not found or deleted: {ProductId}", query.ProductId);
            return new GetProductByIdResponse(null);
        }

        if (product.TenantId != _tenantContext.TenantId)
        {
            _logger.LogWarning("Unauthorized access to product: {ProductId}", query.ProductId);
            throw new UnauthorizedAccessException($"Not authorized to access this resource.");
        }

        var productDto = MapToProductDetailDto(product);
        _logger.LogInformation("Product detail retrieved successfully: {ProductId}", query.ProductId);

        return new GetProductByIdResponse(productDto);
    }

    private static ProductDetailDto MapToProductDetailDto(dynamic product)
    {
        // Map variants
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
                    CostPrice: null, // Not available in variant data
                    Attributes: v.Attributes != null ? BuildAttributesDictionary(v.Attributes) : new Dictionary<string, string>(),
                    QuantityOnHand: v.StockQuantity ?? 0,
                    IsAvailable: (v.IsActive ?? false) && (v.StockQuantity ?? 0) > 0,
                    CreatedAtUtc: DateTime.UtcNow)); // Note: ProductVariant doesn't track CreatedAtUtc; using current time
            }
        }

        // Map images
        var images = new List<ProductImageDto>();
        if (product.Images != null && product.Images.Count > 0)
        {
            var imagesToAdd = new List<(dynamic image, int order)>();
            foreach (var i in product.Images)
            {
                if (!(i.IsDeleted ?? false))
                {
                    imagesToAdd.Add((i, i.DisplayOrder ?? 0));
                }
            }

            foreach (var (i, _) in imagesToAdd.OrderBy(x => x.order))
            {
                images.Add(new ProductImageDto(
                    Id: i.Id,
                    Url: i.Url,
                    AltText: i.AltText,
                    DisplayOrder: i.DisplayOrder ?? 0,
                    IsPrimary: i.IsPrimary ?? false,
                    CreatedAtUtc: DateTime.UtcNow)); // Note: ProductImage doesn't track CreatedAtUtc; using current time
            }
        }

        // Map attributes
        var attributes = BuildAttributesDictionary(product.Attributes);

        // Map tags
        var tags = BuildTagsList(product.Tags);

        return new ProductDetailDto(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Sku: product.Sku,
            BasePrice: product.Price,
            CostPrice: product.CostPrice,
            CurrencyCode: "USD",
            IsAvailable: product.IsAvailable ?? false,
            QuantityOnHand: 0, // Will be populated from inventory tracking when available
            ReorderLevel: 0, // Will be populated from inventory when available
            CategoryId: product.CategoryId,
            CategoryName: "", // Will be populated from category relationship when available
            Variants: variants,
            Images: images,
            Attributes: attributes,
            Tags: tags,
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
            {
                result[a.Name] = a.Value;
            }
        }
        return result;
    }

    private static List<string> BuildTagsList(dynamic tags)
    {
        var result = new List<string>();
        if (tags != null)
        {
            foreach (var t in tags)
            {
                result.Add(t.TagValue ?? string.Empty);
            }
        }
        return result;
    }
}
