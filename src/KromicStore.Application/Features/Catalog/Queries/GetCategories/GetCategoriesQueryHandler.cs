using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;

namespace KromicStore.Application.Features.Catalog.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;

    public GetCategoriesQueryHandler(
        ICategoryRepository categoryRepository,
        ITenantContext tenantContext,
        ILogger<GetCategoriesQueryHandler> logger)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetCategoriesResponse> Handle(
        GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving categories: Skip={Skip}, Take={Take}, ParentId={ParentId}",
            query.Skip, query.Take, query.ParentCategoryId);

        var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);

        var categories = allCategories
            .Where(c => c.TenantId == _tenantContext.TenantId)
            .Where(c => !c.IsDeleted)
            .Where(c => query.ParentCategoryId == null || c.ParentCategoryId == query.ParentCategoryId)
            .OrderBy(c => c.DisplayOrder)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(MapToCategoryDto)
            .ToList();

        _logger.LogInformation("Retrieved {Count} categories", categories.Count);

        return new GetCategoriesResponse(categories);
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
            CreatedAtUtc: category.CreatedOnUtc,
            ModifiedAtUtc: category.ModifiedOnUtc);
    }
}
