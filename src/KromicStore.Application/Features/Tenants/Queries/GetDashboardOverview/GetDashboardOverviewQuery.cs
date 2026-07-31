using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetDashboardOverview;

/// <summary>
/// Query to retrieve tenant dashboard overview with key metrics.
/// Returns aggregated data: orders, revenue, customers, products, low stock, pending orders, today's sales.
/// </summary>
public sealed class GetDashboardOverviewQuery : IRequest<DashboardOverviewResponse>
{
    public Guid TenantId { get; set; }
}

public sealed class DashboardOverviewResponse
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveCustomers { get; set; }
    public int LowStockProducts { get; set; }
    public int PendingOrders { get; set; }
    public decimal TodaysSales { get; set; }
}
