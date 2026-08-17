using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetProductImages;

public sealed class GetProductImagesQueryHandler : IRequestHandler<GetProductImagesQuery, GetProductImagesResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductImagesQueryHandler> _logger;

    public GetProductImagesQueryHandler(
        IProductRepository productRepository,
        ILogger<GetProductImagesQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetProductImagesResponse> Handle(
        GetProductImagesQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving images for product: {ProductId}", query.ProductId);

        // EF global query filter already scopes to the current tenant.
        var product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
        {
            _logger.LogWarning("Product not found or deleted: {ProductId}", query.ProductId);
            return new GetProductImagesResponse([]);
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
                    CreatedAtUtc: DateTime.UtcNow))
                .ToList();
        }

        _logger.LogInformation("Retrieved {Count} images for product: {ProductId}", images.Count, query.ProductId);
        return new GetProductImagesResponse(images);
    }
}
