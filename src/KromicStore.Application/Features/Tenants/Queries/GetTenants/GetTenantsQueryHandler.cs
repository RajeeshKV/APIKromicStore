using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Application.Features.Orders.Abstractions;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Queries.GetTenants;

/// <summary>
/// Handler for GetTenantsQuery.
/// Retrieves paginated list of all tenants with optional filtering and search.
/// </summary>
public sealed class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, TenantsPagedResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetTenantsQueryHandler> _logger;

    public GetTenantsQueryHandler(
        ITenantRepository tenantRepository,
        IOrderRepository orderRepository,
        ILogger<GetTenantsQueryHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TenantsPagedResponse> Handle(
        GetTenantsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving tenants: Skip={Skip}, Take={Take}, Status={Status}, Search={Search}",
            request.Skip, request.Take, request.Status, request.Search);

        // Parse status filter if provided
        TenantStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<TenantStatus>(request.Status, ignoreCase: true, out var parsedStatus))
            {
                statusFilter = parsedStatus;
            }
        }

        // Get paginated tenants
        var (tenants, totalCount) = await _tenantRepository.GetAllWithPaginationAsync(
            request.Skip,
            request.Take,
            statusFilter,
            request.Search,
            cancellationToken);

        // Build response with analytics per tenant
        var tenantDtos = new List<TenantSummaryDto>();
        foreach (var tenant in tenants)
        {
            var orderCount = await _orderRepository.GetOrderCountByTenantIdAsync(tenant.Id, cancellationToken);
            var revenue = await _orderRepository.GetRevenueBytTenantIdAsync(tenant.Id, cancellationToken);

            tenantDtos.Add(new TenantSummaryDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                StoreName = tenant.StoreName,
                Slug = tenant.Slug,
                Status = tenant.Status.ToString(),
                CreatedOnUtc = tenant.CreatedOnUtc,
                OrderCount = orderCount,
                TotalRevenue = revenue
            });
        }

        _logger.LogInformation(
            "Retrieved {Count} tenants from {TotalCount} total",
            tenantDtos.Count, totalCount);

        return new TenantsPagedResponse
        {
            Tenants = tenantDtos,
            TotalCount = totalCount,
            Skip = request.Skip,
            Take = request.Take
        };
    }
}
