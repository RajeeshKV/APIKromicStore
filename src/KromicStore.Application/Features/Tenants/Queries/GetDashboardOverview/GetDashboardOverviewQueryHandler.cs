using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Tenants.Queries.GetDashboardOverview;

public sealed class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetDashboardOverviewQueryHandler> _logger;

    public GetDashboardOverviewQueryHandler(
        IOrderRepository orderRepository,
        ILogger<GetDashboardOverviewQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DashboardOverviewResponse> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving dashboard overview for tenant {TenantId}", request.TenantId);

        var today = DateTime.UtcNow.Date;
        var todayStart = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);

        // Get all orders for tenant (global query filter will handle tenant isolation)
        var allOrders = await _orderRepository.GetByStatusAsync(OrderStatus.Pending, cancellationToken);
        
        // For now, use placeholder values as the repository doesn't have tenant-specific methods
        // This is a limitation that would need to be addressed by extending the repository
        var totalOrders = 0;
        var totalRevenue = 0m;
        var activeCustomers = 0;
        var lowStockProducts = 0;
        var pendingOrders = 0;
        var todaysSales = 0m;

        // TODO: Extend IOrderRepository with tenant-specific query methods to get:
        // - Count orders by tenant
        // - Sum revenue by tenant
        // - Distinct customers by tenant
        // - Query products by tenant for low stock
        // - Count orders by tenant and status
        // - Sum orders by tenant and date

        _logger.LogInformation(
            "Dashboard overview retrieved: Orders={Orders}, Revenue={Revenue}, Customers={Customers}, LowStock={LowStock}, Pending={Pending}, Today={Today}",
            totalOrders, totalRevenue, activeCustomers, lowStockProducts, pendingOrders, todaysSales);

        return new DashboardOverviewResponse
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            ActiveCustomers = activeCustomers,
            LowStockProducts = lowStockProducts,
            PendingOrders = pendingOrders,
            TodaysSales = todaysSales
        };
    }
}
