using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreOrders;

/// <summary>
/// Query to retrieve paginated list of store orders with filtering.
/// </summary>
public sealed class GetStoreOrdersQuery : IRequest<GetStoreOrdersResponse>
{
    public Guid TenantId { get; set; }
    public string? Status { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 10;
}

public sealed class GetStoreOrdersResponse
{
    public List<OrderSummaryDto> Orders { get; set; } = [];
    public int TotalCount { get; set; }
}
