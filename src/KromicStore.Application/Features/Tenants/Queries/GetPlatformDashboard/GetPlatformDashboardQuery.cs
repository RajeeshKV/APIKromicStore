using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetPlatformDashboard;

/// <summary>
/// Query to retrieve platform-wide dashboard metrics for Super Users.
/// Returns aggregated data: active tenants, trial tenants, total revenue, orders, customers.
/// </summary>
public sealed class GetPlatformDashboardQuery : IRequest<PlatformDashboardResponse>
{
}

public sealed class PlatformDashboardResponse
{
    public int ActiveTenants { get; set; }
    public int TrialTenants { get; set; }
    public int SuspendedTenants { get; set; }
    public int ArchivedTenants { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
}
