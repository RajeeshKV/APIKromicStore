using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, SearchProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SearchProductsQueryHandler> _logger;

    public SearchProductsQueryHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        ILogger<SearchProductsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SearchProductsResponse> Handle(
        SearchProductsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching products: SearchText={SearchText}, Skip={Skip}, Take={Take}, CategoryId={CategoryId}",
            query.SearchText, query.Skip, query.Take, query.CategoryId);

        if (string.IsNullOrWhiteSpace(query.SearchText))
        {
            _logger.LogWarning("Search text is empty or whitespace");
            return new SearchProductsResponse([]);
        }

        // Note: Full-text search will be implemented when SearchService is injected into Application layer
        var allProducts = await _productRepository.GetAllAsync(cancellationToken);

        var normalizedSearch = query.SearchText.Trim().ToLowerInvariant();

        var searchResults = allProducts
            .Where(p => p.TenantId == _tenantContext.TenantId)
            .Where(p => !p.IsDeleted)
            .Where(p => query.CategoryId == null || p.CategoryId == query.CategoryId)
            .Where(p =>
                p.Name.ToLower().Contains(normalizedSearch) ||
                p.Description?.ToLower().Contains(normalizedSearch) == true ||
                p.Sku.ToLower().Contains(normalizedSearch))
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(p => MapToProductSearchResultDto(p, normalizedSearch))
            .ToList();

        _logger.LogInformation("Search completed: Found {Count} products", searchResults.Count);

        return new SearchProductsResponse(searchResults);
    }

    private static ProductSearchResultDto MapToProductSearchResultDto(dynamic product, string searchText = "")
    {
        // Note: Tags will be mapped when tag relationship is available
        var tags = new List<string>();
        
        // Calculate basic relevance score based on search text match position
        float relevanceScore = 1.0f;
        if (!string.IsNullOrEmpty(searchText))
        {
            var normalized = searchText.ToLower();
            if (product.Name.ToLower().StartsWith(normalized)) 
                relevanceScore = 3.0f;
            else if (product.Name.ToLower().Contains(normalized)) 
                relevanceScore = 2.5f;
            else if (product.Description?.ToLower().Contains(normalized) == true) 
                relevanceScore = 2.0f;
            else if (product.Sku.ToLower().Contains(normalized)) 
                relevanceScore = 1.5f;
        }
        
        return new ProductSearchResultDto(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description?.Length > 100
                ? product.Description.Substring(0, 100) + "..."
                : product.Description,
            Sku: product.Sku,
            BasePrice: product.Price,
            CurrencyCode: "USD",
            CategoryName: "", // Will be populated from category relationship when available
            Tags: tags,
            IsAvailable: product.IsAvailable ?? false,
            RelevanceScore: relevanceScore);
    }
}
