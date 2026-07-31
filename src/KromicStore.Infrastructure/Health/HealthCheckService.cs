using KromicStore.Application.Common.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Health;

/// <summary>
/// Service that aggregates health checks from all application dependencies
/// and produces a comprehensive health check response.
/// </summary>
public sealed class HealthCheckService : IHealthCheckService
{
    private readonly Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService _healthCheckService;
    private readonly ApplicationStartupState _startupState;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService healthCheckService,
        ApplicationStartupState startupState,
        ILogger<HealthCheckService> logger)
    {
        _healthCheckService = healthCheckService;
        _startupState = startupState;
        _logger = logger;
    }

    public async Task<HealthCheckResponse> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var overallStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var timestamp = DateTime.UtcNow;

        try
        {
            // Get health report from the registered health checks
            var report = await _healthCheckService.CheckHealthAsync(cancellationToken);
            overallStopwatch.Stop();

            // Convert health check entries to service health statuses
            var serviceStatuses = report.Entries
                .Select(kvp => new ServiceHealthStatus
                {
                    Name = kvp.Key,
                    Status = kvp.Value.Status.ToString(),
                    Duration = kvp.Value.Duration.TotalMilliseconds >= 0
                        ? (long)kvp.Value.Duration.TotalMilliseconds
                        : null,
                    Message = kvp.Value.Description
                })
                .ToList();

            // Determine overall status
            var overallStatus = DetermineOverallStatus(report.Status);

            _logger.LogInformation(
                "Health check completed with status: {Status} (completed in {DurationMs}ms)",
                overallStatus,
                overallStopwatch.ElapsedMilliseconds);

            return new HealthCheckResponse
            {
                Status = overallStatus,
                Timestamp = timestamp.ToString("O"),
                Version = GetApplicationVersion(),
                Environment = GetEnvironment(),
                Services = serviceStatuses,
                TraceId = System.Diagnostics.Activity.Current?.Id ?? "unknown"
            };
        }
        catch (Exception ex)
        {
            overallStopwatch.Stop();
            _logger.LogError(ex, "Health check failed after {DurationMs}ms", overallStopwatch.ElapsedMilliseconds);

            return new HealthCheckResponse
            {
                Status = "Unhealthy",
                Timestamp = timestamp.ToString("O"),
                Version = GetApplicationVersion(),
                Environment = GetEnvironment(),
                Services = new[]
                {
                    new ServiceHealthStatus
                    {
                        Name = "Application",
                        Status = "Unhealthy",
                        Duration = overallStopwatch.ElapsedMilliseconds,
                        Message = $"Health check failed: {ex.Message}"
                    }
                },
                TraceId = System.Diagnostics.Activity.Current?.Id ?? "unknown"
            };
        }
    }

    /// <summary>
    /// Determines the overall health status based on individual health check results.
    /// </summary>
    private static string DetermineOverallStatus(HealthStatus status) => status switch
    {
        HealthStatus.Healthy => "Healthy",
        HealthStatus.Degraded => "Degraded",
        HealthStatus.Unhealthy => "Unhealthy",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the application version from assembly.
    /// </summary>
    private static string GetApplicationVersion()
    {
        var version = typeof(HealthCheckService).Assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }

    /// <summary>
    /// Gets the current environment name.
    /// </summary>
    private static string GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    }
}
