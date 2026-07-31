using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Tenants.Queries.GetTenantDetail;

/// <summary>
/// Handler for GetTenantDetailQuery.
/// Retrieves detailed tenant information with analytics.
/// </summary>
public sealed class GetTenantDetailQueryHandler : IRequestHandler<GetTenantDetailQuery, TenantDetailResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetTenantDetailQueryHandler> _logger;

    public GetTenantDetailQueryHandler(
        ITenantRepository tenantRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ILogger<GetTenantDetailQueryHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TenantDetailResponse> Handle(
        GetTenantDetailQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving tenant detail for tenant {TenantId}", request.TenantId);

        // Get tenant
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", request.TenantId);
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");
        }

        // Get order statistics
        var totalOrders = await _orderRepository.GetOrderCountByTenantIdAsync(request.TenantId, cancellationToken);
        var activeOrders = (await _orderRepository.GetByTenantIdAsync(request.TenantId, cancellationToken))
            .Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed);
        var totalRevenue = await _orderRepository.GetRevenueBytTenantIdAsync(request.TenantId, cancellationToken);
        var uniqueCustomers = await _orderRepository.GetUniqueCustomerCountByTenantIdAsync(request.TenantId, cancellationToken);

        // Calculate average order value
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

        // Get product statistics
        var totalProducts = await _productRepository.GetCountByTenantIdAsync(request.TenantId, cancellationToken);
        var lowStockProducts = await _productRepository.GetLowStockCountByTenantIdAsync(request.TenantId, threshold: 10, cancellationToken);

        _logger.LogInformation(
            "Tenant detail retrieved: TotalOrders={TotalOrders}, ActiveOrders={ActiveOrders}, TotalRevenue={TotalRevenue}, UniqueCustomers={UniqueCustomers}, TotalProducts={TotalProducts}, LowStockProducts={LowStockProducts}",
            totalOrders, activeOrders, totalRevenue, uniqueCustomers, totalProducts, lowStockProducts);

        return new TenantDetailResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            StoreName = tenant.StoreName,
            Slug = tenant.Slug,
            Status = tenant.Status.ToString(),
            OwnerUserId = tenant.OwnerUserId,
            CreatedOnUtc = tenant.CreatedOnUtc,
            UpdatedOnUtc = tenant.ModifiedOnUtc,
            TotalOrders = totalOrders,
            ActiveOrders = activeOrders,
            TotalCustomers = uniqueCustomers,
            TotalRevenue = totalRevenue,
            AverageOrderValue = averageOrderValue,
            TotalProducts = totalProducts,
            LowStockProducts = lowStockProducts
        };
    }
}
