using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IApplicationDbContext dbContext,
        ILogger<CreateCategoryCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category: {Name}", command.Name);

        // Check for duplicate slug if provided
        if (!string.IsNullOrEmpty(command.Slug))
        {
            var slugExists = await _categoryRepository.SlugExistsAsync(command.Slug, null, cancellationToken);
            if (slugExists)
            {
                _logger.LogWarning("Duplicate slug attempted: {Slug}", command.Slug);
                throw new InvalidOperationException($"A category with slug '{command.Slug}' already exists.");
            }
        }

        // Create the category aggregate
        var category = Category.Create(
            tenantId: _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved"),
            name: command.Name,
            customSlug: command.Slug,
            description: command.Description,
            parentCategoryId: command.ParentCategoryId,
            displayOrder: command.DisplayOrder,
            isVisible: command.IsVisible,
            imageUrl: command.ImageUrl);

        // Mark as created (with audit info)
        category.MarkCreated(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Add to repository
        _categoryRepository.Add(category);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category created successfully: {CategoryId}", category.Id);

        return new CreateCategoryResponse(
            CategoryId: category.Id,
            Name: category.Name,
            Slug: category.Slug);
    }
}
