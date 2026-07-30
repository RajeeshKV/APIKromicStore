using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, GetCategoryByIdResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(
        ICategoryRepository categoryRepository,
        ITenantContext tenantContext,
        ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetCategoryByIdResponse> Handle(
        GetCategoryByIdQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving category: {CategoryId}", query.CategoryId);

        var category = await _categoryRepository.GetByIdAsync(query.CategoryId, cancellationToken);

        if (category == null || category.IsDeleted)
        {
            _logger.LogWarning("Category not found or deleted: {CategoryId}", query.CategoryId);
            return new GetCategoryByIdResponse(null);
        }

        if (category.TenantId != _tenantContext.TenantId)
        {
            _logger.LogWarning("Unauthorized access to category: {CategoryId}", query.CategoryId);
            throw new UnauthorizedAccessException($"Not authorized to access this resource.");
        }

        var categoryDto = MapToCategoryDto(category);
        _logger.LogInformation("Category retrieved successfully: {CategoryId}", query.CategoryId);

        return new GetCategoryByIdResponse(categoryDto);
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
