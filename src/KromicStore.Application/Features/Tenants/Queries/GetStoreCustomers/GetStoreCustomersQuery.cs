using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreCustomers;

public sealed class GetStoreCustomersQuery : IRequest<StoreCustomersResponse>
{
    public Guid TenantId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
}

public sealed class StoreCustomersResponse
{
    public List<CustomerSummary> Customers { get; set; } = new();
    public int TotalCount { get; set; }
}

public sealed class CustomerSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastOrderDate { get; set; }
}
