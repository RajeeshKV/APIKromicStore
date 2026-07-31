using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Queries.GetPlatformMonitoring;

public sealed class GetPlatformMonitoringQueryHandler : IRequestHandler<GetPlatformMonitoringQuery, PlatformMonitoringResponse>
{
    private readonly ILogger<GetPlatformMonitoringQueryHandler> _logger;

    public GetPlatformMonitoringQueryHandler(ILogger<GetPlatformMonitoringQueryHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PlatformMonitoringResponse> Handle(
        GetPlatformMonitoringQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving platform monitoring status");

        // In a real implementation, this would check actual service health
        var response = new PlatformMonitoringResponse
        {
            HealthStatus = "Healthy",
            DatabaseStatus = "Connected",
            EmailStatus = "Operational",
            CloudinaryStatus = "Connected",
            PaymentGatewayStatus = "Operational",
            BackgroundJobsStatus = "Running",
            QueuedJobs = 0,
            FailedJobs = 0,
            StorageUsedBytes = 536_870_912_000, // 500GB example
            StorageTotalBytes = 1_099_511_627_776, // 1TB
            LastCheckedUtc = DateTime.UtcNow
        };

        response.StorageUsagePercentage = (double)response.StorageUsedBytes / response.StorageTotalBytes * 100;

        // Add sample recent logs
        response.RecentLogs = new List<SystemLog>
        {
            new SystemLog
            {
                OccurredOnUtc = DateTime.UtcNow.AddHours(-1),
                Level = "Info",
                Message = "Background job processed successfully"
            },
            new SystemLog
            {
                OccurredOnUtc = DateTime.UtcNow.AddHours(-2),
                Level = "Info",
                Message = "Email service connected"
            }
        };

        _logger.LogInformation("Platform monitoring status retrieved: Health={Health}",
            response.HealthStatus);

        await Task.CompletedTask;
        return response;
    }
}
