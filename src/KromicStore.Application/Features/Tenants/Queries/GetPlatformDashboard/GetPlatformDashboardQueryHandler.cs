using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Queries.GetPlatformDashboard;

/// <summary>
/// Handler for GetPlatformDashboardQuery.
/// Retrieves platform-wide metrics across all tenants.
/// </summary>
public sealed class GetPlatformDashboardQueryHandler : IRequestHandler<GetPlatformDashboardQuery, PlatformDashboardResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetPlatformDashboardQueryHandler> _logger;

    public GetPlatformDashboardQueryHandler(
        ITenantRepository tenantRepository,
        IOrderRepository orderRepository,
        ILogger<GetPlatformDashboardQueryHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PlatformDashboardResponse> Handle(
        GetPlatformDashboardQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving platform dashboard metrics");

        // Get tenant counts by status
        var activeTenantCount = await _tenantRepository.CountByStatusAsync(TenantStatus.Active, cancellationToken);
        var trialTenantCount = await _tenantRepository.CountByStatusAsync(TenantStatus.Provisioning, cancellationToken);
        var suspendedTenantCount = await _tenantRepository.CountByStatusAsync(TenantStatus.Suspended, cancellationToken);
        var archivedTenantCount = await _tenantRepository.CountByStatusAsync(TenantStatus.Archived, cancellationToken);

        // Get order metrics
        var totalOrderCount = await _orderRepository.GetTotalOrderCountAsync(cancellationToken);
        var totalRevenue = await _orderRepository.GetTotalRevenueAsync(cancellationToken);
        var totalUniqueCustomers = await _orderRepository.GetTotalUniqueCustomerCountAsync(cancellationToken);

        // Calculate average order value
        var averageOrderValue = totalOrderCount > 0 ? totalRevenue / totalOrderCount : 0m;

        _logger.LogInformation(
            "Platform dashboard retrieved: ActiveTenants={ActiveTenants}, TrialTenants={TrialTenants}, SuspendedTenants={SuspendedTenants}, ArchivedTenants={ArchivedTenants}, TotalOrders={TotalOrders}, TotalRevenue={TotalRevenue}, TotalCustomers={TotalCustomers}, AverageOrderValue={AverageOrderValue}",
            activeTenantCount, trialTenantCount, suspendedTenantCount, archivedTenantCount, totalOrderCount, totalRevenue, totalUniqueCustomers, averageOrderValue);

        return new PlatformDashboardResponse
        {
            ActiveTenants = activeTenantCount,
            TrialTenants = trialTenantCount,
            SuspendedTenants = suspendedTenantCount,
            ArchivedTenants = archivedTenantCount,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrderCount,
            TotalCustomers = totalUniqueCustomers,
            AverageOrderValue = averageOrderValue
        };
    }
}
