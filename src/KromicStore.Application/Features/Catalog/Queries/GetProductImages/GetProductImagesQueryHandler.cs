using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetProductImages;

public sealed class GetProductImagesQueryHandler : IRequestHandler<GetProductImagesQuery, GetProductImagesResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetProductImagesQueryHandler> _logger;

    public GetProductImagesQueryHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        ILogger<GetProductImagesQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetProductImagesResponse> Handle(
        GetProductImagesQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving images for product: {ProductId}", query.ProductId);

        var product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
        {
            _logger.LogWarning("Product not found or deleted: {ProductId}", query.ProductId);
            return new GetProductImagesResponse([]);
        }

        if (product.TenantId != _tenantContext.TenantId)
        {
            _logger.LogWarning("Unauthorized access to product: {ProductId}", query.ProductId);
            throw new UnauthorizedAccessException($"Not authorized to access this resource.");
        }

        var images = new List<ProductImageDto>();

        if (product.Images != null && product.Images.Count > 0)
        {
            images = product.Images
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ProductImageDto(
                    Id: i.Id,
                    Url: i.Url,
                    AltText: i.AltText,
                    DisplayOrder: i.DisplayOrder,
                    IsPrimary: i.IsPrimary,
                    CreatedAtUtc: DateTime.UtcNow)) // Note: ProductImage doesn't track CreatedAtUtc; using current time
                .ToList();
        }

        _logger.LogInformation("Retrieved {Count} images for product: {ProductId}", images.Count, query.ProductId);

        return new GetProductImagesResponse(images);
    }
}
