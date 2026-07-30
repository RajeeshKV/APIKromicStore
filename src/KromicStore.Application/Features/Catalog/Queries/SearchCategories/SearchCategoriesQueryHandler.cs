using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.SearchCategories;

public sealed class SearchCategoriesQueryHandler : IRequestHandler<SearchCategoriesQuery, SearchCategoriesResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SearchCategoriesQueryHandler> _logger;

    public SearchCategoriesQueryHandler(
        ICategoryRepository categoryRepository,
        ITenantContext tenantContext,
        ILogger<SearchCategoriesQueryHandler> logger)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SearchCategoriesResponse> Handle(
        SearchCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching categories: SearchText={SearchText}", query.SearchText);

        if (string.IsNullOrWhiteSpace(query.SearchText))
        {
            _logger.LogWarning("Search text is empty or whitespace");
            return new SearchCategoriesResponse([]);
        }

        var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);

        var normalizedSearch = query.SearchText.Trim().ToLowerInvariant();

        var searchResults = allCategories
            .Where(c => c.TenantId == _tenantContext.TenantId)
            .Where(c => !c.IsDeleted)
            .Where(c =>
                c.Name.ToLower().Contains(normalizedSearch) ||
                c.Slug.ToLower().Contains(normalizedSearch))
            .Select(MapToCategoryDto)
            .ToList();

        _logger.LogInformation("Search completed: Found {Count} categories", searchResults.Count);

        return new SearchCategoriesResponse(searchResults);
    }

    private static CategoryDto MapToCategoryDto(dynamic category)
    {
        return new CategoryDto(
            Id: category.Id,
            Name: category.Name,
            Description: category.Description,
            ParentCategoryId: category.ParentCategoryId,
            DisplayOrder: category.DisplayOrder,
            IsActive: category.Status == 0, // CategoryStatus.Active
            Slug: category.Slug,
            ProductCount: 0, // Will be populated from Category.Products relationship when available
            CreatedAtUtc: category.CreatedAtUtc,
            ModifiedAtUtc: category.ModifiedAtUtc);
    }
}
