using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IApplicationDbContext dbContext,
        ILogger<CreateProductCommandHandler> logger,
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

    public async Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product: {Name} (SKU: {Sku})", command.Name, command.Sku);

        // Verify category exists
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category == null)
        {
            _logger.LogWarning("Category not found: {CategoryId}", command.CategoryId);
            throw new InvalidOperationException($"Category with ID {command.CategoryId} not found.");
        }

        // Check for duplicate SKU
        var skuExists = await _productRepository.SkuExistsAsync(command.Sku, null, cancellationToken);
        if (skuExists)
        {
            _logger.LogWarning("Duplicate SKU attempted: {Sku}", command.Sku);
            throw new InvalidOperationException($"A product with SKU '{command.Sku}' already exists.");
        }

        // Check for duplicate slug if provided
        if (!string.IsNullOrEmpty(command.CustomSlug))
        {
            var slugExists = await _productRepository.SlugExistsAsync(command.CustomSlug, null, cancellationToken);
            if (slugExists)
            {
                _logger.LogWarning("Duplicate slug attempted: {Slug}", command.CustomSlug);
                throw new InvalidOperationException($"A product with slug '{command.CustomSlug}' already exists.");
            }
        }

        // Parse enums
        var productType = Enum.Parse<ProductType>(command.ProductType ?? "Physical");
        var status = Enum.Parse<ProductStatus>(command.Status ?? "Draft");

        // Create the product aggregate
        var product = Product.Create(
            tenantId: _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved"),
            categoryId: command.CategoryId,
            sku: command.Sku,
            name: command.Name,
            customSlug: command.CustomSlug,
            shortDescription: command.ShortDescription,
            description: command.Description,
            productType: productType,
            status: status,
            price: command.Price,
            compareAtPrice: command.CompareAtPrice,
            costPrice: command.CostPrice,
            weight: command.Weight,
            length: command.Length,
            width: command.Width,
            height: command.Height,
            isFeatured: command.IsFeatured,
            trackInventory: command.TrackInventory,
            taxable: command.Taxable);

        // Add attributes if provided
        if (command.Attributes != null && command.Attributes.Count > 0)
        {
            foreach (var (key, value) in command.Attributes)
            {
                product.AddAttribute(key, value);
            }
        }

        // Add tags if provided
        if (command.Tags != null && command.Tags.Count > 0)
        {
            foreach (var tag in command.Tags)
            {
                product.AddTag(tag);
            }
        }

        // Mark as created (with audit info)
        product.MarkCreated(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Add to repository
        _productRepository.Add(product);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created successfully: {ProductId}", product.Id);

        return new CreateProductResponse(
            ProductId: product.Id,
            Name: product.Name,
            Sku: product.Sku,
            Slug: product.Slug);
    }
}
