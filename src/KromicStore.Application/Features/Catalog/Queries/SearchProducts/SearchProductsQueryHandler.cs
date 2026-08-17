using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, SearchProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<SearchProductsQueryHandler> _logger;

    public SearchProductsQueryHandler(
        IProductRepository productRepository,
        ILogger<SearchProductsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
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

        // EF global query filter already scopes results to the current tenant.
        var allProducts = await _productRepository.GetAllAsync(cancellationToken);
        var normalizedSearch = query.SearchText.Trim().ToLowerInvariant();

        var searchResults = allProducts
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
        var tags = new List<string>();

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
            CategoryName: "",
            Tags: tags,
            IsAvailable: product.IsAvailable ?? false,
            RelevanceScore: relevanceScore);
    }
}
