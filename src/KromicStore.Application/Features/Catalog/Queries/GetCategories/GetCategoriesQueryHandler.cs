using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Catalog.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<GetCategoriesQueryHandler> _logger;

    public GetCategoriesQueryHandler(
        ICategoryRepository categoryRepository,
        ILogger<GetCategoriesQueryHandler> logger)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetCategoriesResponse> Handle(
        GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving categories: Skip={Skip}, Take={Take}, ParentId={ParentId}",
            query.Skip, query.Take, query.ParentCategoryId);

        // EF global query filter already scopes results to the current tenant.
        var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);

        var categories = allCategories
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
            IsActive: category.Status == 0,
            Slug: category.Slug,
            ProductCount: 0,
            CreatedAtUtc: category.CreatedOnUtc,
            ModifiedAtUtc: category.ModifiedOnUtc);
    }
}
