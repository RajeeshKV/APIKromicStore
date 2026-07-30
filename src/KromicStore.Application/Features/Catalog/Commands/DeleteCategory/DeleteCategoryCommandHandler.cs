using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IApplicationDbContext dbContext,
        ILogger<DeleteCategoryCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting category: {CategoryId}", command.CategoryId);

        // Get the category
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", command.CategoryId);
            throw new InvalidOperationException($"Category with ID {command.CategoryId} not found.");
        }

        // Soft delete the category
        category.SoftDelete(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category deleted successfully: {CategoryId}", category.Id);

        return new DeleteCategoryResponse(
            CategoryId: category.Id,
            Message: "Category has been deleted successfully.");
    }
}
