using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetPlatformAnalytics;

public sealed class GetPlatformAnalyticsQuery : IRequest<PlatformAnalyticsResponse>
{
    public DateTime StartDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public string GroupBy { get; set; } = "daily"; // daily, weekly, monthly
}

public sealed class PlatformAnalyticsResponse
{
    public decimal TotalRevenue { get; set; }
    public int NewTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TrialConversions { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public decimal ChurnRate { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalCustomers { get; set; }
    public List<DailyMetric> DailyMetrics { get; set; } = new();
    public List<TenantMetric> TopTenants { get; set; } = new();
}

public sealed class DailyMetric
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public int Customers { get; set; }
    public int NewTenants { get; set; }
}

public sealed class TenantMetric
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public int Customers { get; set; }
}
