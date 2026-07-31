using MediatR;

namespace KromicStore.Application.StoreOperations.Queries.GetInventoryDashboard;

/// <summary>
/// Query to retrieve inventory dashboard with key metrics.
/// </summary>
public sealed class GetInventoryDashboardQuery : IRequest<GetInventoryDashboardResponse>
{
}

public sealed class LowStockProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
}

public sealed class GetInventoryDashboardResponse
{
    public int TotalProducts { get; set; }
    public int TotalUnitsInStock { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int LowStockProductCount { get; set; }
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    public int PendingAdjustmentsCount { get; set; }
    public int ApprovedAdjustmentsCount { get; set; }
}
