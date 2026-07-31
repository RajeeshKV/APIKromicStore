using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Catalog.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetLowStockProducts;

public sealed class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, LowStockProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetLowStockProductsQueryHandler> _logger;

    public GetLowStockProductsQueryHandler(IProductRepository productRepository, ILogger<GetLowStockProductsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LowStockProductsResponse> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving low stock products for tenant {TenantId} with threshold {Threshold}",
            request.TenantId, request.ThresholdQty);

        var products = await _productRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        
        var lowStockProducts = products
            .Where(p => p.Inventory != null && p.Inventory.GetAvailableStock() < request.ThresholdQty)
            .Select(p => new LowStockProduct
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                CurrentStock = p.Inventory?.GetAvailableStock() ?? 0,
                ThresholdQty = request.ThresholdQty
            })
            .OrderBy(p => p.CurrentStock)
            .ToList();

        return new LowStockProductsResponse { Products = lowStockProducts };
    }
}
