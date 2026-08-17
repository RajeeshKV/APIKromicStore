using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        ILogger<GetProductsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetProductsResponse> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving products: Skip={Skip}, Take={Take}, CategoryId={CategoryId}, Status={Status}",
            query.Skip, query.Take, query.CategoryId, query.Status);

        IEnumerable<dynamic> products;

        if (query.CategoryId.HasValue)
        {
            var categoryProducts = await _productRepository.GetByCategoryIdAsync(query.CategoryId.Value, cancellationToken);
            products = categoryProducts;
        }
        else
        {
            products = await _productRepository.GetAllAsync(cancellationToken);
        }

        // EF global query filter already scopes to the current tenant — no in-memory re-filter needed.
        var productList = products
            .Where(p => !p.IsDeleted)
            .Where(p => query.Status == null || (int)p.Status == query.Status)
            .OrderByDescending(p => p.CreatedOnUtc)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(MapToProductCardDto)
            .ToList();

        _logger.LogInformation("Retrieved {Count} products", productList.Count);

        return new GetProductsResponse(productList);
    }

    private static ProductCardDto MapToProductCardDto(dynamic product)
    {
        // Note: Tags will be mapped when tag relationship is available
        var tags = new List<string>();
        
        return new ProductCardDto(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Sku: product.Sku,
            BasePrice: product.Price,
            CurrencyCode: "USD",
            IsAvailable: product.IsAvailable ?? false,
            QuantityOnHand: 0, // Will be populated from inventory tracking when available
            CategoryId: product.CategoryId,
            CategoryName: "", // Will be populated from category relationship when available
            Tags: tags,
            CreatedAtUtc: product.CreatedOnUtc);
    }
}
