namespace KromicStore.Application.Common.Models;

/// <summary>
/// Represents the health status of a dependency service.
/// </summary>
public record ServiceHealthStatus
{
    /// <summary>
    /// Name of the service (e.g., "Database", "Cache", "Application").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Health status: Healthy, Degraded, or Unhealthy.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Time taken to complete the health check in milliseconds.
    /// </summary>
    public long? Duration { get; init; }

    /// <summary>
    /// Optional message providing details about the health status.
    /// </summary>
    public string? Message { get; init; }
}

/// <summary>
/// Overall health check response returned by GET /health endpoint.
/// </summary>
public record HealthCheckResponse
{
    /// <summary>
    /// Overall health status: Healthy, Degraded, or Unhealthy.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// UTC timestamp when the health check was performed.
    /// </summary>
    public required string Timestamp { get; init; }

    /// <summary>
    /// Application version.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Current deployment environment (Development, Staging, Production).
    /// </summary>
    public required string Environment { get; init; }

    /// <summary>
    /// Health status of all checked services.
    /// </summary>
    public required IEnumerable<ServiceHealthStatus> Services { get; init; }

    /// <summary>
    /// Unique trace identifier for correlation.
    /// </summary>
    public required string TraceId { get; init; }
}
