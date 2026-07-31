using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreAnalytics;

public sealed class GetStoreAnalyticsQueryHandler : IRequestHandler<GetStoreAnalyticsQuery, StoreAnalyticsResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetStoreAnalyticsQueryHandler> _logger;

    public GetStoreAnalyticsQueryHandler(IOrderRepository orderRepository, ILogger<GetStoreAnalyticsQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StoreAnalyticsResponse> Handle(GetStoreAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving analytics for tenant {TenantId} from {StartDate} to {EndDate}",
            request.TenantId, request.StartDate, request.EndDate);

        var totalRevenue = await _orderRepository.GetRevenueBytTenantIdAsync(request.TenantId, cancellationToken);
        var orderCount = await _orderRepository.GetOrderCountByTenantIdAsync(request.TenantId, cancellationToken);
        var customerCount = await _orderRepository.GetUniqueCustomerCountByTenantIdAsync(request.TenantId, cancellationToken);

        var averageOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0m;

        return new StoreAnalyticsResponse
        {
            TotalRevenue = totalRevenue,
            OrderCount = orderCount,
            CustomerCount = customerCount,
            AverageOrderValue = averageOrderValue,
            ConversionRate = 0, // Requires visitor/session tracking
            ProductsSold = orderCount
        };
    }
}
