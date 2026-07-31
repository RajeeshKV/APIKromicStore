using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Orders.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreCustomers;

public sealed class GetStoreCustomersQueryHandler : IRequestHandler<GetStoreCustomersQuery, StoreCustomersResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetStoreCustomersQueryHandler> _logger;

    public GetStoreCustomersQueryHandler(IOrderRepository orderRepository, ILogger<GetStoreCustomersQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<StoreCustomersResponse> Handle(GetStoreCustomersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving customers for tenant {TenantId}, skip={Skip}, take={Take}",
            request.TenantId, request.Skip, request.Take);

        var orders = await _orderRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        // Aggregate customer data from orders
        var customerData = orders
            .GroupBy(o => o.CustomerId)
            .Select(g => new CustomerSummary
            {
                Id = g.Key,
                Name = "Customer", // Customer name not available on Order - TODO: Link with CustomerProfile
                Email = string.Empty, // Email not available on Order
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(o => o.GrandTotal),
                LastOrderDate = g.Max(o => o.CreatedOnUtc)
            })
            .OrderByDescending(c => c.LastOrderDate)
            .ToList();

        var totalCount = customerData.Count;
        var paginatedCustomers = customerData
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return new StoreCustomersResponse
        {
            Customers = paginatedCustomers,
            TotalCount = totalCount
        };
    }
}
