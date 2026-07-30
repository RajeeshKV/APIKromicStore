using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.AdjustInventory;

public sealed class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand, AdjustInventoryResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<AdjustInventoryCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public AdjustInventoryCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<AdjustInventoryCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<AdjustInventoryResponse> Handle(AdjustInventoryCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adjusting inventory for product: {ProductId} by {Quantity}", 
            command.ProductId, command.QuantityAdjustment);

        // Get the product
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Check if product has inventory
        if (product.Inventory == null)
        {
            _logger.LogWarning("Product has no inventory: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} does not have inventory tracking.");
        }

        // Adjust the inventory
        product.Inventory.AdjustAvailableQuantity(command.QuantityAdjustment, command.Reason);

        _logger.LogInformation("Inventory adjusted for product {ProductId}. New available quantity: {NewQuantity}, Reserved: {Reserved}", 
            product.Id, product.Inventory.AvailableQuantity, product.Inventory.ReservedQuantity);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdjustInventoryResponse(
            ProductId: product.Id,
            NewAvailableQuantity: product.Inventory.AvailableQuantity,
            ReservedQuantity: product.Inventory.ReservedQuantity,
            Message: "Inventory has been adjusted successfully.");
    }
}
