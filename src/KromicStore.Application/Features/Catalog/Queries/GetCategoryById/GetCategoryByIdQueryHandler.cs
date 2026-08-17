using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, GetCategoryByIdResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(
        ICategoryRepository categoryRepository,
        ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetCategoryByIdResponse> Handle(
        GetCategoryByIdQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving category: {CategoryId}", query.CategoryId);

        // EF global query filter already scopes to the current tenant.
        // A category belonging to a different tenant simply won't be found (returns null).
        var category = await _categoryRepository.GetByIdAsync(query.CategoryId, cancellationToken);

        if (category == null || category.IsDeleted)
        {
            _logger.LogWarning("Category not found or deleted: {CategoryId}", query.CategoryId);
            return new GetCategoryByIdResponse(null);
        }

        _logger.LogInformation("Category retrieved successfully: {CategoryId}", query.CategoryId);
        return new GetCategoryByIdResponse(MapToCategoryDto(category));
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
