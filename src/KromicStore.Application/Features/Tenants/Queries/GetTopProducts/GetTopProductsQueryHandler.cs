using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetTopProducts;

public sealed class GetTopProductsQueryHandler : IRequestHandler<GetTopProductsQuery, TopProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetTopProductsQueryHandler> _logger;

    public GetTopProductsQueryHandler(IProductRepository productRepository, IOrderRepository orderRepository, ILogger<GetTopProductsQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TopProductsResponse> Handle(GetTopProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving top {Limit} products for tenant {TenantId}", request.Limit, request.TenantId);

        var products = await _productRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        var orders = await _orderRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        // Aggregate sales by product from orders
        var productSales = new Dictionary<Guid, (int Count, decimal Revenue)>();
        
        foreach (var order in orders)
        {
            foreach (var item in order.Items)
            {
                if (productSales.ContainsKey(item.ProductId))
                {
                    var current = productSales[item.ProductId];
                    productSales[item.ProductId] = (current.Count + 1, current.Revenue + item.UnitPrice * item.Quantity);
                }
                else
                {
                    productSales[item.ProductId] = (1, item.UnitPrice * item.Quantity);
                }
            }
        }

        var topProducts = products
            .Where(p => productSales.ContainsKey(p.Id))
            .Select(p => 
            {
                var (count, revenue) = productSales[p.Id];
                return new TopProduct
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    SalesCount = count,
                    Revenue = revenue
                };
            })
            .OrderByDescending(p => p.Revenue)
            .Take(request.Limit)
            .ToList();

        return new TopProductsResponse { Products = topProducts };
    }
}
