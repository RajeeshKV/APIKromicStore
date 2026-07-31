using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetPlatformMonitoring;

public sealed class GetPlatformMonitoringQuery : IRequest<PlatformMonitoringResponse>
{
}

public sealed class PlatformMonitoringResponse
{
    public string HealthStatus { get; set; } = "Healthy"; // Healthy, Degraded, Critical
    public string DatabaseStatus { get; set; } = "Connected";
    public string EmailStatus { get; set; } = "Operational";
    public string CloudinaryStatus { get; set; } = "Connected";
    public string PaymentGatewayStatus { get; set; } = "Operational";
    public string BackgroundJobsStatus { get; set; } = "Running";
    public int QueuedJobs { get; set; }
    public int FailedJobs { get; set; }
    public long StorageUsedBytes { get; set; }
    public long StorageTotalBytes { get; set; } = 1_099_511_627_776; // 1TB default
    public double StorageUsagePercentage { get; set; }
    public DateTime LastCheckedUtc { get; set; } = DateTime.UtcNow;
    public List<SystemLog> RecentLogs { get; set; } = new();
}

public sealed class SystemLog
{
    public DateTime OccurredOnUtc { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
