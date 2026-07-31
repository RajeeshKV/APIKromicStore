using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Storefront.Queries.ListFeaturedProducts;

public sealed class ListFeaturedProductsQueryHandler : IRequestHandler<ListFeaturedProductsQuery, ListFeaturedProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ListFeaturedProductsQueryHandler> _logger;

    public ListFeaturedProductsQueryHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        ILogger<ListFeaturedProductsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ListFeaturedProductsResponse> Handle(
        ListFeaturedProductsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving featured products for tenant {TenantId}, Take={Take}", _tenantContext.TenantId, request.Take);

        var allProducts = await _productRepository.GetAllAsync(cancellationToken);

        var products = allProducts
            .Where(p => p.TenantId == _tenantContext.TenantId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAtUtc)
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
                QuantityOnHand: 0, // Will be populated from inventory tracking when available
                CollectionName: null))
            .ToList();

        _logger.LogInformation("Retrieved {ProductCount} featured products for tenant {Tenant}", products.Count, _tenantContext.TenantId);

        return new ListFeaturedProductsResponse(products);
    }
}
