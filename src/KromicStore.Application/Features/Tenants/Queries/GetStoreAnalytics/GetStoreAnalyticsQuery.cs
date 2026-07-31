using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreAnalytics;

/// <summary>
/// Query to retrieve detailed store analytics for a date range.
/// Returns revenue, order count, customer count, average order value, and conversion metrics.
/// </summary>
public sealed class GetStoreAnalyticsQuery : IRequest<StoreAnalyticsResponse>
{
    public Guid TenantId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class StoreAnalyticsResponse
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal ConversionRate { get; set; }
}
