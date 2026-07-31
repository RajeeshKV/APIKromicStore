using KromicStore.Application.Common.Models;
using KromicStore.Infrastructure.Health;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KromicStore.API.Controllers;

/// <summary>
/// Health check endpoints for monitoring application status.
/// Used by Render, Docker, Kubernetes, load balancers, and monitoring systems.
/// Both GET and HEAD endpoints are publicly accessible (no authentication required).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IHealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Gets comprehensive health check information including all service statuses.
    /// 
    /// Used by: Render, Docker, Kubernetes, load balancers, monitoring systems.
    /// 
    /// Response codes:
    /// - 200 OK: Application is healthy or degraded
    /// - 503 Service Unavailable: Application is unhealthy
    /// 
    /// Returns a structured JSON response with overall status and per-service status details.
    /// </summary>
    /// <returns>Detailed health check response with service statuses.</returns>
    [HttpGet]
    public async Task<ActionResult<HealthCheckResponse>> Get(CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await _healthCheckService.CheckHealthAsync(cancellationToken);
            sw.Stop();

            _logger.LogInformation(
                "Health check GET request completed with status: {Status} in {DurationMs}ms",
                response.Status,
                sw.ElapsedMilliseconds);

            // Return 503 if unhealthy, 200 for healthy/degraded
            if (response.Status == "Unhealthy")
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "Health check GET request failed after {DurationMs}ms",
                sw.ElapsedMilliseconds);

            var errorResponse = new HealthCheckResponse
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow.ToString("O"),
                Version = GetApplicationVersion(),
                Environment = GetEnvironment(),
                Services = new[]
                {
                    new ServiceHealthStatus
                    {
                        Name = "Health Check Service",
                        Status = "Unhealthy",
                        Duration = sw.ElapsedMilliseconds,
                        Message = "Health check service encountered an error"
                    }
                },
                TraceId = HttpContext.TraceIdentifier
            };

            return StatusCode(StatusCodes.Status503ServiceUnavailable, errorResponse);
        }
    }

    /// <summary>
    /// HEAD request handler for health checks.
    /// Returns the same status code as GET without a response body.
    /// 
    /// Useful for lightweight monitoring systems that only need the status code.
    /// This is commonly used by Render, Docker, and Kubernetes for rapid health checks.
    /// 
    /// Response codes:
    /// - 200 OK: Application is healthy or degraded
    /// - 503 Service Unavailable: Application is unhealthy
    /// </summary>
    /// <returns>Status code only (no body).</returns>
    [HttpHead]
    public async Task<IActionResult> Head(CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await _healthCheckService.CheckHealthAsync(cancellationToken);
            sw.Stop();

            _logger.LogDebug(
                "Health check HEAD request completed with status: {Status} in {DurationMs}ms",
                response.Status,
                sw.ElapsedMilliseconds);

            // Return 503 if unhealthy, 200 for healthy/degraded
            if (response.Status == "Unhealthy")
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "Health check HEAD request failed after {DurationMs}ms",
                sw.ElapsedMilliseconds);

            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }

    /// <summary>
    /// Gets the application version from assembly.
    /// </summary>
    private static string GetApplicationVersion()
    {
        var version = typeof(HealthController).Assembly.GetName().Version;
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
