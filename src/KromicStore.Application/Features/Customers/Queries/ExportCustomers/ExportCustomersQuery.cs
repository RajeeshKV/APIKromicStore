using MediatR;

namespace KromicStore.Application.Features.Customers.Queries.ExportCustomers;

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

/// <summary>
/// Query to export customers to CSV format.
/// </summary>
public sealed record ExportCustomersQuery : IRequest<ExportCustomersResult>;

/// <summary>
/// Result containing CSV data and metadata for customer export.
/// </summary>
public sealed record ExportCustomersResult(
    byte[] CsvData,
    string FileName,
    int TotalCustomers
);
