using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetPlatformAnalytics;

public sealed class GetPlatformAnalyticsQueryHandler : IRequestHandler<GetPlatformAnalyticsQuery, PlatformAnalyticsResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetPlatformAnalyticsQueryHandler> _logger;

    public GetPlatformAnalyticsQueryHandler(
        ITenantRepository tenantRepository,
        IOrderRepository orderRepository,
        ILogger<GetPlatformAnalyticsQueryHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PlatformAnalyticsResponse> Handle(
        GetPlatformAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving platform analytics from {StartDate} to {EndDate}",
            request.StartDate, request.EndDate);

        var response = new PlatformAnalyticsResponse
        {
            TotalRevenue = await _orderRepository.GetTotalRevenueAsync(cancellationToken),
            ActiveTenants = await _tenantRepository.CountByStatusAsync(Domain.Tenants.TenantStatus.Active, cancellationToken),
            TotalOrders = await _orderRepository.GetTotalOrderCountAsync(cancellationToken),
            TotalCustomers = await _orderRepository.GetTotalUniqueCustomerCountAsync(cancellationToken),
            NewTenants = 0, // Would require date tracking on Tenant creation
            TrialConversions = 0, // Would require trial-to-active conversion tracking
            MonthlyRecurringRevenue = 0, // Would require subscription tracking
            ChurnRate = 0, // Would require historical tenant status tracking
            AverageOrderValue = 0 // Calculated below
        };

        if (response.TotalOrders > 0)
            response.AverageOrderValue = response.TotalRevenue / response.TotalOrders;

        // Daily metrics would require order timestamp aggregation
        response.DailyMetrics = new List<DailyMetric>();

        // Top tenants by revenue
        var allTenants = await _tenantRepository.GetAllAsync(cancellationToken);
        var topTenants = new List<TenantMetric>();
        foreach (var tenant in allTenants.OrderByDescending(t => t.TotalRevenue).Take(10))
        {
            var revenue = await _orderRepository.GetRevenueBytTenantIdAsync(tenant.Id, cancellationToken);
            var orderCount = await _orderRepository.GetOrderCountByTenantIdAsync(tenant.Id, cancellationToken);
            var customerCount = await _orderRepository.GetUniqueCustomerCountByTenantIdAsync(tenant.Id, cancellationToken);

            topTenants.Add(new TenantMetric
            {
                TenantId = tenant.Id,
                TenantName = tenant.StoreName,
                Revenue = revenue,
                Orders = orderCount,
                Customers = customerCount
            });
        }
        response.TopTenants = topTenants;

        _logger.LogInformation("Platform analytics retrieved: Revenue={Revenue}, Orders={Orders}, Customers={Customers}",
            response.TotalRevenue, response.TotalOrders, response.TotalCustomers);

        return response;
    }
}
