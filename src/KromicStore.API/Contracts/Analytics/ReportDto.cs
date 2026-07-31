namespace KromicStore.API.Contracts.Analytics;

/// <summary>
/// DTO representing an analytics report.
/// </summary>
public class ReportDto
{
    /// <summary>
    /// Report type (Sales, Orders, Customers, Products, Revenue).
    /// </summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Report title for display.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Report data as key-value pairs.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// When report was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Date range start.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Date range end.
    /// </summary>
    public DateTime EndDate { get; set; }
}
