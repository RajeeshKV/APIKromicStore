using MediatR;

namespace KromicStore.Application.Features.Orders.Queries.ExportOrders;

/// <summary>
/// DTO for exporting order data to CSV.
/// </summary>
public sealed class OrderExportDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Query to export orders within a date range to CSV format.
/// </summary>
public sealed record ExportOrdersQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<ExportOrdersResult>;

/// <summary>
/// Result containing CSV data and metadata for order export.
/// </summary>
public sealed record ExportOrdersResult(
    byte[] CsvData,
    string FileName,
    int TotalOrders
);
