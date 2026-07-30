using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateProductCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IApplicationDbContext dbContext,
        ILogger<UpdateProductCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<UpdateProductResponse> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating product: {ProductId}", command.ProductId);

        // Get the product
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Verify category exists if provided
        if (command.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId.Value, cancellationToken);
            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryId}", command.CategoryId);
                throw new InvalidOperationException($"Category with ID {command.CategoryId} not found.");
            }
        }

        // Check for duplicate SKU if provided
        if (!string.IsNullOrEmpty(command.Sku))
        {
            var skuExists = await _productRepository.SkuExistsAsync(command.Sku, command.ProductId, cancellationToken);
            if (skuExists)
            {
                _logger.LogWarning("Duplicate SKU attempted: {Sku}", command.Sku);
                throw new InvalidOperationException($"A product with SKU '{command.Sku}' already exists.");
            }
        }

        // Check for duplicate slug if provided
        if (!string.IsNullOrEmpty(command.CustomSlug))
        {
            var slugExists = await _productRepository.SlugExistsAsync(command.CustomSlug, command.ProductId, cancellationToken);
            if (slugExists)
            {
                _logger.LogWarning("Duplicate slug attempted: {Slug}", command.CustomSlug);
                throw new InvalidOperationException($"A product with slug '{command.CustomSlug}' already exists.");
            }
        }

        // Parse enums if provided
        ProductStatus? status = null;
        if (!string.IsNullOrEmpty(command.Status))
        {
            status = Enum.Parse<ProductStatus>(command.Status);
        }

        // Update the product
        product.Update(
            categoryId: command.CategoryId,
            sku: command.Sku,
            name: command.Name,
            customSlug: command.CustomSlug,
            shortDescription: command.ShortDescription,
            description: command.Description,
            status: status,
            price: command.Price,
            compareAtPrice: command.CompareAtPrice,
            costPrice: command.CostPrice,
            weight: command.Weight,
            length: command.Length,
            width: command.Width,
            height: command.Height,
            isFeatured: command.IsFeatured,
            taxable: command.Taxable);

        // Mark as modified
        product.MarkModified(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product updated successfully: {ProductId}", product.Id);

        return new UpdateProductResponse(
            ProductId: product.Id,
            Name: product.Name,
            Sku: product.Sku,
            Slug: product.Slug);
    }
}
