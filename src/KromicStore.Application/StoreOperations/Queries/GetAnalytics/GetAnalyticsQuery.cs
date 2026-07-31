using MediatR;

namespace KromicStore.Application.StoreOperations.Queries.GetAnalytics;

/// <summary>
/// Query to retrieve store analytics and performance metrics.
/// </summary>
public sealed class GetAnalyticsQuery : IRequest<GetAnalyticsResponse>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed class GetAnalyticsResponse
{
    // Order metrics
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    
    // Fulfillment metrics
    public int PendingFulfillments { get; set; }
    public int CompletedFulfillments { get; set; }
    public double AverageFulfillmentTime { get; set; } // In days
    
    // Return metrics
    public int TotalReturnRequests { get; set; }
    public int ApprovedReturns { get; set; }
    public int RejectedReturns { get; set; }
    public decimal TotalRefundAmount { get; set; }
    
    // Inventory metrics
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int StockOuts { get; set; }
    public decimal InventoryTurnoverRate { get; set; }
    
    // Customer metrics
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public double CustomerRetentionRate { get; set; }
}
