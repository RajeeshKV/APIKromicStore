using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateVariant;

public sealed class UpdateVariantCommandHandler : IRequestHandler<UpdateVariantCommand, UpdateVariantResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateVariantCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateVariantCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<UpdateVariantCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<UpdateVariantResponse> Handle(UpdateVariantCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating variant: {VariantId} for product {ProductId}", command.VariantId, command.ProductId);

        // Get the product
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Get the variant from the product
        var variant = product.Variants.FirstOrDefault(v => v.Id == command.VariantId);
        if (variant == null)
        {
            _logger.LogWarning("Variant not found: {VariantId} in product {ProductId}", command.VariantId, command.ProductId);
            throw new InvalidOperationException($"Variant with ID {command.VariantId} not found in product {command.ProductId}.");
        }

        // Update the variant
        variant.Update(
            name: command.Name,
            priceAdjustment: command.PriceAdjustment,
            attributes: command.Attributes,
            isActive: command.IsActive);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Variant updated successfully: {VariantId}", variant.Id);

        return new UpdateVariantResponse(
            VariantId: variant.Id,
            ProductId: product.Id,
            Name: variant.Name,
            Sku: variant.Sku);
    }
}
