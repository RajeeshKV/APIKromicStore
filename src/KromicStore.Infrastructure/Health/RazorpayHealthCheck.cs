using KromicStore.Infrastructure.Services.Payments;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.Health;

/// <summary>
/// Health check for Razorpay payment gateway.
/// Verifies that the payment gateway is properly configured and can communicate with Razorpay API.
/// </summary>
public class RazorpayHealthCheck : IHealthCheck
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<RazorpayHealthCheck> _logger;

    public RazorpayHealthCheck(IPaymentGateway paymentGateway, ILogger<RazorpayHealthCheck> logger)
    {
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking Razorpay payment gateway health");

            // Attempt to perform a health check with the payment gateway
            var isHealthy = await _paymentGateway.HealthCheckAsync(cancellationToken);

            if (isHealthy)
            {
                _logger.LogDebug("Razorpay payment gateway health check passed");
                return HealthCheckResult.Healthy("Razorpay payment gateway is operational");
            }

            _logger.LogWarning("Razorpay payment gateway health check failed");
            return HealthCheckResult.Unhealthy("Razorpay payment gateway is not responding");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay health check raised an exception");
            return HealthCheckResult.Unhealthy("Razorpay payment gateway health check failed", ex);
        }
    }
}

