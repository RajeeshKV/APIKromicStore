using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.Catalog.ValueObjects;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.DuplicateProduct;

public sealed class DuplicateProductCommandHandler : IRequestHandler<DuplicateProductCommand, DuplicateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DuplicateProductCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DuplicateProductCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<DuplicateProductCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<DuplicateProductResponse> Handle(DuplicateProductCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Duplicating product: {ProductId}", command.ProductId);

        // Get the original product
        var originalProduct = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (originalProduct == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Check for duplicate SKU
        var skuExists = await _productRepository.SkuExistsAsync(command.NewSku, null, cancellationToken);
        if (skuExists)
        {
            _logger.LogWarning("Duplicate SKU attempted: {Sku}", command.NewSku);
            throw new InvalidOperationException($"A product with SKU '{command.NewSku}' already exists.");
        }

        // Check for duplicate slug if provided
        if (!string.IsNullOrEmpty(command.NewSlug))
        {
            var slugExists = await _productRepository.SlugExistsAsync(command.NewSlug, null, cancellationToken);
            if (slugExists)
            {
                _logger.LogWarning("Duplicate slug attempted: {Slug}", command.NewSlug);
                throw new InvalidOperationException($"A product with slug '{command.NewSlug}' already exists.");
            }
        }

        // Generate slug
        var slugObj = Slug.Create(command.NewSlug, command.NewName);

        // Create duplicate product
        var duplicatedProduct = Product.Create(
            tenantId: originalProduct.TenantId,
            categoryId: originalProduct.CategoryId,
            sku: command.NewSku,
            name: command.NewName,
            customSlug: command.NewSlug,
            shortDescription: originalProduct.ShortDescription,
            description: originalProduct.Description,
            productType: originalProduct.ProductType,
            status: ProductStatus.Draft,
            price: originalProduct.Price,
            compareAtPrice: originalProduct.CompareAtPrice,
            costPrice: originalProduct.CostPrice,
            weight: originalProduct.Weight,
            length: originalProduct.Length,
            width: originalProduct.Width,
            height: originalProduct.Height,
            isFeatured: false,
            trackInventory: originalProduct.TrackInventory,
            taxable: originalProduct.Taxable);

        // Copy attributes
        foreach (var attribute in originalProduct.Attributes)
        {
            duplicatedProduct.AddAttribute(attribute.AttributeName, attribute.AttributeValue);
        }

        // Copy tags
        foreach (var tag in originalProduct.Tags)
        {
            duplicatedProduct.AddTag(tag.Tag);
        }

        // Mark as created
        duplicatedProduct.MarkCreated(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Add to repository
        _productRepository.Add(duplicatedProduct);

        // Raise domain event on original product
        originalProduct.Duplicate(command.NewSku, command.NewName, command.NewSlug);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product duplicated successfully. Original: {OriginalId}, Duplicate: {DuplicateId}", 
            originalProduct.Id, duplicatedProduct.Id);

        return new DuplicateProductResponse(
            DuplicatedProductId: duplicatedProduct.Id,
            NewSku: duplicatedProduct.Sku,
            NewName: duplicatedProduct.Name,
            NewSlug: duplicatedProduct.Slug);
    }
}
