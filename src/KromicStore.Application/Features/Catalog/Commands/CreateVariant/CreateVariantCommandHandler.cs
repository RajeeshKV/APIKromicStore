using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.CreateVariant;

public sealed class CreateVariantCommandHandler : IRequestHandler<CreateVariantCommand, CreateVariantResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateVariantCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateVariantCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<CreateVariantCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<CreateVariantResponse> Handle(CreateVariantCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating variant for product: {ProductId}", command.ProductId);

        // Get the product
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Build the variant SKU
        var variantSku = $"{product.Sku}-{command.SkuSuffix}";

        // Check for duplicate SKU
        var skuExists = await _productRepository.SkuExistsAsync(variantSku, null, cancellationToken);
        if (skuExists)
        {
            _logger.LogWarning("Duplicate variant SKU attempted: {Sku}", variantSku);
            throw new InvalidOperationException($"A product variant with SKU '{variantSku}' already exists.");
        }

        // Create the variant
        var variant = ProductVariant.Create(
            productId: product.Id,
            sku: variantSku,
            name: command.Name,
            priceAdjustment: command.PriceAdjustment,
            attributes: command.Attributes,
            stockQuantity: command.StockQuantity);

        // Add variant to product
        product.AddVariant(
            skuSuffix: command.SkuSuffix,
            name: command.Name,
            priceAdjustment: command.PriceAdjustment,
            attributes: command.Attributes);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Variant created successfully: {VariantId} for product {ProductId}", variant.Id, product.Id);

        return new CreateVariantResponse(
            VariantId: variant.Id,
            ProductId: product.Id,
            Name: variant.Name,
            Sku: variant.Sku);
    }
}
