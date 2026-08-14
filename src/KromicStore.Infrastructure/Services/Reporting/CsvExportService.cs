using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace KromicStore.Infrastructure.Services.Reporting;

/// <summary>
/// Service for exporting data to CSV format.
/// Handles serialization of orders, customers, and other entities to CSV.
/// </summary>
public interface ICsvExportService
{
    /// <summary>
    /// Exports orders to CSV format.
    /// </summary>
    byte[] ExportOrders(IEnumerable<OrderExportDto> orders);

    /// <summary>
    /// Exports customers to CSV format.
    /// </summary>
    byte[] ExportCustomers(IEnumerable<CustomerExportDto> customers);
}

/// <summary>
/// Implementation of CSV export service using CsvHelper library.
/// </summary>
public sealed class CsvExportService : ICsvExportService
{
    public byte[] ExportOrders(IEnumerable<OrderExportDto> orders)
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        csvWriter.WriteHeader<OrderExportDto>();
        csvWriter.NextRecord();

        foreach (var order in orders)
        {
            csvWriter.WriteRecord(order);
            csvWriter.NextRecord();
        }

        streamWriter.Flush();
        return memoryStream.ToArray();
    }

    public byte[] ExportCustomers(IEnumerable<CustomerExportDto> customers)
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        csvWriter.WriteHeader<CustomerExportDto>();
        csvWriter.NextRecord();

        foreach (var customer in customers)
        {
            csvWriter.WriteRecord(customer);
            csvWriter.NextRecord();
        }

        streamWriter.Flush();
        return memoryStream.ToArray();
    }
}

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
/// DTO for exporting customer data to CSV.
/// </summary>
public sealed class CustomerExportDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime? LastOrderDate { get; set; }
}
