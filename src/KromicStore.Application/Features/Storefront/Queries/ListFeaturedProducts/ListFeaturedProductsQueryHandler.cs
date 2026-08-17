using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Storefront.Queries.ListFeaturedProducts;

public sealed class ListFeaturedProductsQueryHandler : IRequestHandler<ListFeaturedProductsQuery, ListFeaturedProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ListFeaturedProductsQueryHandler> _logger;

    public ListFeaturedProductsQueryHandler(
        IProductRepository productRepository,
        ILogger<ListFeaturedProductsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ListFeaturedProductsResponse> Handle(
        ListFeaturedProductsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving featured products, Take={Take}", request.Take);

        // EF global query filter already scopes results to the current tenant.
        var allProducts = await _productRepository.GetAllAsync(cancellationToken);

        var products = allProducts
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Take(request.Take)
            .Select(p => new FeaturedProductDto(
                Id: p.Id,
                Name: p.Name,
                Description: p.Description,
                BasePrice: p.Price,
                DiscountedPrice: p.CompareAtPrice,
                ImageUrl: p.Images.FirstOrDefault()?.Url,
                Sku: p.Sku,
                IsAvailable: !p.IsDeleted,
                QuantityOnHand: 0,
                CollectionName: null))
            .ToList();

        _logger.LogInformation("Retrieved {ProductCount} featured products", products.Count);

        return new ListFeaturedProductsResponse(products);
    }
}
