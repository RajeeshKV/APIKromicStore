using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GenerateRevenueReport;

public sealed class GenerateRevenueReportQuery : IRequest<RevenueReportResponse>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ExportFormat { get; set; } = "json"; // json, csv, pdf
}

public sealed class RevenueReportResponse
{
    public string ReportId { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<RevenueByTenantDto> TenantBreakdown { get; set; } = new();
    public string ExportUrl { get; set; } = string.Empty;
    public string ExportFormat { get; set; } = string.Empty;
}

public sealed class RevenueByTenantDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal Percentage { get; set; }
}
