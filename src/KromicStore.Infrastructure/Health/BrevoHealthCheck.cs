using KromicStore.Infrastructure.Services.Email;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Health;

/// <summary>
/// Health check for Brevo email service.
/// Verifies that the email service is properly configured and can communicate with Brevo API.
/// </summary>
public class BrevoHealthCheck : IHealthCheck
{
    private readonly IEmailService _emailService;
    private readonly ILogger<BrevoHealthCheck> _logger;

    public BrevoHealthCheck(IEmailService emailService, ILogger<BrevoHealthCheck> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking Brevo email service health");

            // Attempt to perform a health check with the email service
            var isHealthy = await _emailService.HealthCheckAsync(cancellationToken);

            if (isHealthy)
            {
                _logger.LogDebug("Brevo email service health check passed");
                return HealthCheckResult.Healthy("Brevo email service is operational");
            }

            _logger.LogWarning("Brevo email service health check failed");
            return HealthCheckResult.Unhealthy("Brevo email service is not responding");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Brevo health check raised an exception");
            return HealthCheckResult.Unhealthy("Brevo email service health check failed", ex);
        }
    }
}

