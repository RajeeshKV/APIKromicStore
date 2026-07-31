using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetTenantDetail;

/// <summary>
/// Query to retrieve detailed information about a specific tenant.
/// Returns tenant data with analytics and statistics.
/// </summary>
public sealed class GetTenantDetailQuery : IRequest<TenantDetailResponse>
{
    public Guid TenantId { get; set; }
}

public sealed class TenantDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? OwnerUserId { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    
    // Analytics
    public int TotalOrders { get; set; }
    public int ActiveOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
}
