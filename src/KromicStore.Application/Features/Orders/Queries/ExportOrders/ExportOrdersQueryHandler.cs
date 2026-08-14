using KromicStore.Application.Common.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Orders.Queries.ExportOrders;

/// <summary>
/// Handles export of orders to CSV format.
/// Retrieves orders within date range and exports to CSV.
/// </summary>
public sealed class ExportOrdersQueryHandler
    : IRequestHandler<ExportOrdersQuery, ExportOrdersResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<ExportOrdersQueryHandler> _logger;

    public ExportOrdersQueryHandler(
        IApplicationDbContext db,
        ILogger<ExportOrdersQueryHandler> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExportOrdersResult> Handle(
        ExportOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (request.EndDate < request.StartDate)
        {
            throw new InvalidOperationException("End date must be after start date");
        }

        // Retrieve orders within date range
        var orders = await _db.Orders
            .Where(o => o.CreatedOnUtc >= request.StartDate && o.CreatedOnUtc <= request.EndDate)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedOnUtc)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Exporting {OrderCount} orders from {StartDate} to {EndDate}",
            orders.Count, request.StartDate, request.EndDate);

        // Map to export DTOs
        var orderExports = orders.Select(o => new OrderExportDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerEmail = "", // Customer email would require additional lookup
            CustomerName = "N/A", // Customer name would require additional lookup
            Total = o.GrandTotal,
            Status = o.Status.ToString(),
            ItemCount = o.Items.Count,
            CreatedDate = o.CreatedOnUtc
        }).ToList();

        // Generate CSV manually (without external service)
        var csvData = GenerateCsv(orderExports);
        var fileName = $"Orders_Export_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}.csv";

        _logger.LogInformation("Orders exported successfully: {FileName}, Size: {Size} bytes",
            fileName, csvData.Length);

        return new ExportOrdersResult(
            CsvData: csvData,
            FileName: fileName,
            TotalOrders: orderExports.Count
        );
    }

    private static byte[] GenerateCsv(IEnumerable<OrderExportDto> orders)
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, System.Text.Encoding.UTF8);

        // Write header
        streamWriter.WriteLine("Id,OrderNumber,CustomerEmail,CustomerName,Total,Status,ItemCount,CreatedDate");

        // Write data rows
        foreach (var order in orders)
        {
            var line = $"\"{order.Id}\",\"{order.OrderNumber}\",\"{order.CustomerEmail}\",\"{order.CustomerName}\"," +
                      $"{order.Total},\"{order.Status}\",{order.ItemCount},\"{order.CreatedDate:O}\"";
            streamWriter.WriteLine(line);
        }

        streamWriter.Flush();
        return memoryStream.ToArray();
    }
}
