using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.RestoreCategory;

public sealed class RestoreCategoryCommandHandler : IRequestHandler<RestoreCategoryCommand, RestoreCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<RestoreCategoryCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public RestoreCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IApplicationDbContext dbContext,
        ILogger<RestoreCategoryCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<RestoreCategoryResponse> Handle(RestoreCategoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Restoring category: {CategoryId}", command.CategoryId);

        // Get the category (including soft-deleted ones)
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", command.CategoryId);
            throw new InvalidOperationException($"Category with ID {command.CategoryId} not found.");
        }

        if (!category.IsDeleted)
        {
            _logger.LogWarning("Category is not deleted: {CategoryId}", command.CategoryId);
            throw new InvalidOperationException($"Category with ID {command.CategoryId} is not deleted.");
        }

        // Restore the category
        category.Restore(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category restored successfully: {CategoryId}", category.Id);

        return new RestoreCategoryResponse(
            CategoryId: category.Id,
            Message: "Category has been restored successfully.");
    }
}
