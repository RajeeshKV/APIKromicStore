using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreOrders;

public sealed class GetStoreOrdersQueryHandler : IRequestHandler<GetStoreOrdersQuery, StoreOrdersResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetStoreOrdersQueryHandler> _logger;

    public GetStoreOrdersQueryHandler(IOrderRepository orderRepository, ILogger<GetStoreOrdersQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StoreOrdersResponse> Handle(GetStoreOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving orders for tenant {TenantId}, skip={Skip}, take={Take}, status={Status}",
            request.TenantId, request.Skip, request.Take, request.Status);

        // Get orders with pagination and status filtering
        var orders = await _orderRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        
        var filteredOrders = orders.AsEnumerable();
        if (!string.IsNullOrEmpty(request.Status))
            filteredOrders = filteredOrders.Where(o => o.Status.ToString() == request.Status);

        var totalCount = filteredOrders.Count();
        var paginatedOrders = filteredOrders
            .OrderByDescending(o => o.CreatedOnUtc)
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(o => new OrderSummary
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Total = o.GrandTotal,
                Status = o.Status.ToString(),
                CreatedOnUtc = o.CreatedOnUtc,
                CustomerName = string.Empty
            })
            .ToList();

        return new StoreOrdersResponse
        {
            Orders = paginatedOrders,
            TotalCount = totalCount
        };
    }
}
