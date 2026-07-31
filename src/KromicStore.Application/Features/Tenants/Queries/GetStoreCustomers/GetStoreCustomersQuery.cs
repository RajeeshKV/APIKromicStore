using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetStoreCustomers;

/// <summary>
/// Query to retrieve paginated list of customers for a store with purchase history.
/// </summary>
public sealed class GetStoreCustomersQuery : IRequest<GetStoreCustomersResponse>
{
    public Guid TenantId { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
    public string? Search { get; set; }
}

public sealed class CustomerSummaryDto
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

public sealed class GetStoreCustomersResponse
{
    public List<CustomerSummaryDto> Customers { get; set; } = [];
    public int TotalCount { get; set; }
}
