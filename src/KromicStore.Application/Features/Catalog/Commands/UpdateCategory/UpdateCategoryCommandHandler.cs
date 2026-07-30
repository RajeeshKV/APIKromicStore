using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IApplicationDbContext dbContext,
        ILogger<UpdateCategoryCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category: {CategoryId}", command.CategoryId);

        // Get the category
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", command.CategoryId);
            throw new InvalidOperationException($"Category with ID {command.CategoryId} not found.");
        }

        // Check for duplicate slug if provided
        if (!string.IsNullOrEmpty(command.Slug))
        {
            var slugExists = await _categoryRepository.SlugExistsAsync(command.Slug, command.CategoryId, cancellationToken);
            if (slugExists)
            {
                _logger.LogWarning("Duplicate slug attempted: {Slug}", command.Slug);
                throw new InvalidOperationException($"A category with slug '{command.Slug}' already exists.");
            }
        }

        // Update the category
        category.Update(
            name: command.Name,
            customSlug: command.Slug,
            description: command.Description,
            parentCategoryId: command.ParentCategoryId,
            displayOrder: command.DisplayOrder,
            isVisible: command.IsVisible,
            imageUrl: command.ImageUrl);

        // Mark as modified
        category.MarkModified(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category updated successfully: {CategoryId}", category.Id);

        return new UpdateCategoryResponse(
            CategoryId: category.Id,
            Name: category.Name,
            Slug: category.Slug);
    }
}
