using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreAnalytics;

public sealed class GetStoreAnalyticsQuery : IRequest<StoreAnalyticsResponse>
{
    public Guid TenantId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public sealed class StoreAnalyticsResponse
{
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public int CustomerCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal ConversionRate { get; set; }
    public int ProductsSold { get; set; }
}
