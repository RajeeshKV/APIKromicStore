using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetInventory;

public sealed class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, GetInventoryResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetInventoryQueryHandler> _logger;

    public GetInventoryQueryHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        ILogger<GetInventoryQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetInventoryResponse> Handle(
        GetInventoryQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving inventory for product: {ProductId}", query.ProductId);

        var product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
        {
            _logger.LogWarning("Product not found or deleted: {ProductId}", query.ProductId);
            return new GetInventoryResponse(null);
        }

        if (product.TenantId != _tenantContext.TenantId)
        {
            _logger.LogWarning("Unauthorized access to product: {ProductId}", query.ProductId);
            throw new UnauthorizedAccessException($"Not authorized to access this resource.");
        }

        // Note: Full inventory tracking is pending integration with ProductInventory entity
        var inventoryDto = new InventoryDto(
            ProductId: product.Id,
            Sku: product.Sku,
            QuantityOnHand: 0, // Will be populated from ProductInventory
            ReorderLevel: 0, // Will be populated from ProductInventory
            QuantityReserved: 0, // Will be populated from ProductInventory
            AvailableQuantity: 0, // Will be calculated (QOH - Reserved)
            IsInStock: true, // Will be recalculated based on QOH > 0
            IsBelowReorderLevel: false, // Will check if Available <= ReorderLevel
            LastAdjustedAtUtc: null); // Will be populated from ProductInventory

        _logger.LogInformation("Inventory retrieved for product: {ProductId}", query.ProductId);

        return new GetInventoryResponse(inventoryDto);
    }
}
