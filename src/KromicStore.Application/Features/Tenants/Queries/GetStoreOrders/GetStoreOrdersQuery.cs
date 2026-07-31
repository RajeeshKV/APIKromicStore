using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreOrders;

public sealed class GetStoreOrdersQuery : IRequest<StoreOrdersResponse>
{
    public Guid TenantId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
    public string? Status { get; set; }
}

public sealed class StoreOrdersResponse
{
    public List<OrderSummary> Orders { get; set; } = new();
    public int TotalCount { get; set; }
}

public sealed class OrderSummary
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}
