using KromicStore.Infrastructure.Services.Media;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Health;

/// <summary>
/// Health check for Cloudinary media service.
/// Verifies that the media service is properly configured and can communicate with Cloudinary API.
/// </summary>
public class CloudinaryHealthCheck : IHealthCheck
{
    private readonly IMediaService _mediaService;
    private readonly ILogger<CloudinaryHealthCheck> _logger;

    public CloudinaryHealthCheck(IMediaService mediaService, ILogger<CloudinaryHealthCheck> logger)
    {
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking Cloudinary media service health");

            // Attempt to perform a health check with the media service
            var isHealthy = await _mediaService.HealthCheckAsync(cancellationToken);

            if (isHealthy)
            {
                _logger.LogDebug("Cloudinary media service health check passed");
                return HealthCheckResult.Healthy("Cloudinary media service is operational");
            }

            _logger.LogWarning("Cloudinary media service health check failed");
            return HealthCheckResult.Unhealthy("Cloudinary media service is not responding");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloudinary health check raised an exception");
            return HealthCheckResult.Unhealthy("Cloudinary media service health check failed", ex);
        }
    }
}

