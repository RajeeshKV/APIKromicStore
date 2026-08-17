using MediatR;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Queries.GetInventory;

public sealed class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, GetInventoryResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetInventoryQueryHandler> _logger;

    public GetInventoryQueryHandler(
        IProductRepository productRepository,
        ILogger<GetInventoryQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetInventoryResponse> Handle(
        GetInventoryQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving inventory for product: {ProductId}", query.ProductId);

        // EF global query filter already scopes to the current tenant.
        var product = await _productRepository.GetByIdAsync(query.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
        {
            _logger.LogWarning("Product not found or deleted: {ProductId}", query.ProductId);
            return new GetInventoryResponse(null);
        }

        var inventoryDto = new InventoryDto(
            ProductId: product.Id,
            Sku: product.Sku,
            QuantityOnHand: 0,
            ReorderLevel: 0,
            QuantityReserved: 0,
            AvailableQuantity: 0,
            IsInStock: true,
            IsBelowReorderLevel: false,
            LastAdjustedAtUtc: null);

        _logger.LogInformation("Inventory retrieved for product: {ProductId}", query.ProductId);
        return new GetInventoryResponse(inventoryDto);
    }
}
