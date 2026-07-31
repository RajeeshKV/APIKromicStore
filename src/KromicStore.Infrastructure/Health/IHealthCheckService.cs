using KromicStore.Application.Common.Models;

namespace KromicStore.Infrastructure.Health;

/// <summary>
/// Service responsible for aggregating health checks from all application dependencies
/// and producing a comprehensive health check response.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Performs a comprehensive health check on all application dependencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A HealthCheckResponse containing the status of all services.</returns>
    Task<HealthCheckResponse> CheckHealthAsync(CancellationToken cancellationToken = default);
}
