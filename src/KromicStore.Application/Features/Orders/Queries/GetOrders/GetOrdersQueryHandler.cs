using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Orders.Entities;

namespace KromicStore.Application.Features.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrdersQueryHandler> _logger;

    public GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        ILogger<GetOrdersQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetOrdersResponse> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty && request.CustomerId == Guid.Empty)
            throw new ArgumentException("Either TenantId or CustomerId must be provided");

        // Normalize pagination
        var skip = Math.Max(0, request.Skip);
        var take = Math.Max(1, Math.Min(100, request.Take));

        var orders = new List<OrderSummaryDto>();
        int totalCount;

        try
        {
            if (request.CustomerId.HasValue && request.CustomerId.Value != Guid.Empty)
            {
                // Customer portal: get customer's orders
                _logger.LogInformation("Retrieving orders for customer {CustomerId}", request.CustomerId.Value);
                
                var customerOrders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId.Value, cancellationToken);
                
                // Apply status filter if provided
                var filtered = customerOrders.ToList();
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var status))
                    {
                        filtered = filtered.Where(o => o.Status == status).ToList();
                    }
                }

                // Order by descending date
                filtered = filtered.OrderByDescending(o => o.CreatedOnUtc).ToList();

                totalCount = filtered.Count;
                orders = filtered
                    .Skip(skip)
                    .Take(take)
                    .Select(o => new OrderSummaryDto(
                        Id: o.Id,
                        OrderNumber: o.OrderNumber,
                        OrderDateUtc: o.CreatedOnUtc,
                        Total: o.GrandTotal,
                        Status: o.Status.ToString(),
                        ItemCount: o.Items.Count))
                    .ToList();
            }
            else
            {
                // Tenant portal: get store's orders
                _logger.LogInformation("Retrieving orders for tenant {TenantId}", request.TenantId);
                
                // For tenant portal, we'd typically fetch orders where TenantId matches
                // This would require additional repository method or a separate tenant-specific query
                // For now, return empty as tenant-specific order retrieval needs more context
                totalCount = 0;
            }

            _logger.LogInformation(
                "Retrieved {Count} orders ({Total} total) with skip={Skip}, take={Take}",
                orders.Count, totalCount, skip, take);

            return new GetOrdersResponse
            {
                Orders = orders,
                TotalCount = totalCount,
                Skip = skip,
                Take = take
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
}
