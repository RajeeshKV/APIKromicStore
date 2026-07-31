using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetTopProducts;

/// <summary>
/// Query to retrieve top selling products within a date range.
/// </summary>
public sealed class GetTopProductsQuery : IRequest<GetTopProductsResponse>
{
    public Guid TenantId { get; set; }
    public int Take { get; set; } = 10;
    public int Days { get; set; } = 30;
}

public sealed class TopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public sealed class GetTopProductsResponse
{
    public List<TopProductDto> Products { get; set; } = [];
}
