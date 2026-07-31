using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GenerateRevenueReport;

public sealed class GenerateRevenueReportQueryHandler : IRequestHandler<GenerateRevenueReportQuery, RevenueReportResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GenerateRevenueReportQueryHandler> _logger;

    public GenerateRevenueReportQueryHandler(
        ITenantRepository tenantRepository,
        IOrderRepository orderRepository,
        ILogger<GenerateRevenueReportQueryHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RevenueReportResponse> Handle(
        GenerateRevenueReportQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating revenue report from {StartDate} to {EndDate} in {Format}",
            request.StartDate, request.EndDate, request.ExportFormat);

        var totalRevenue = await _orderRepository.GetTotalRevenueAsync(cancellationToken);
        var totalOrders = await _orderRepository.GetTotalOrderCountAsync(cancellationToken);

        var response = new RevenueReportResponse
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
            ExportFormat = request.ExportFormat
        };

        // Generate tenant breakdown
        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
        foreach (var tenant in tenants)
        {
            var tenantRevenue = await _orderRepository.GetRevenueBytTenantIdAsync(tenant.Id, cancellationToken);
            var tenantOrders = await _orderRepository.GetOrderCountByTenantIdAsync(tenant.Id, cancellationToken);

            if (tenantOrders > 0)
            {
                var percentage = totalRevenue > 0 ? (tenantRevenue / totalRevenue) * 100 : 0;
                response.TenantBreakdown.Add(new RevenueByTenantDto
                {
                    TenantId = tenant.Id,
                    TenantName = tenant.StoreName,
                    Revenue = tenantRevenue,
                    OrderCount = tenantOrders,
                    Percentage = percentage
                });
            }
        }

        // Sort by revenue descending
        response.TenantBreakdown = response.TenantBreakdown.OrderByDescending(x => x.Revenue).ToList();

        _logger.LogInformation("Revenue report generated: Total={Total}, Orders={Orders}",
            response.TotalRevenue, response.TotalOrders);

        return response;
    }
}
