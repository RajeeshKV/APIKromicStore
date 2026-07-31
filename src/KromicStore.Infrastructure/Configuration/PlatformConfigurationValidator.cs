using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KromicStore.Infrastructure.Configuration;

/// <summary>
/// Validates platform-level configuration settings during application startup.
/// Ensures all required configurations are present and valid, including external service integrations.
/// </summary>
public sealed class PlatformConfigurationValidator
{
    private readonly MultiTenancyOptions _multiTenancyOptions;
    private readonly CorsOptions _corsOptions;
    private readonly BrevoOptions _brevoOptions;
    private readonly CloudinaryOptions _cloudinaryOptions;
    private readonly RazorpayOptions _razorpayOptions;
    private readonly ILogger<PlatformConfigurationValidator> _logger;

    public PlatformConfigurationValidator(
        IOptions<MultiTenancyOptions> multiTenancyOptions,
        IOptions<CorsOptions> corsOptions,
        IOptions<BrevoOptions> brevoOptions,
        IOptions<CloudinaryOptions> cloudinaryOptions,
        IOptions<RazorpayOptions> razorpayOptions,
        ILogger<PlatformConfigurationValidator> logger)
    {
        _multiTenancyOptions = multiTenancyOptions.Value;
        _corsOptions = corsOptions.Value;
        _brevoOptions = brevoOptions.Value;
        _cloudinaryOptions = cloudinaryOptions.Value;
        _razorpayOptions = razorpayOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Validates all platform configuration.
    /// Throws InvalidOperationException if validation fails.
    /// </summary>
    public void ValidateAndLog()
    {
        _logger.LogInformation("Validating platform configuration...");

        // Validate multi-tenancy configuration
        var (multiTenancyValid, multiTenancyError) = _multiTenancyOptions.Validate();
        if (!multiTenancyValid)
        {
            _logger.LogError("Multi-tenancy configuration validation failed: {Error}", multiTenancyError);
            throw new InvalidOperationException($"Multi-tenancy configuration is invalid: {multiTenancyError}");
        }

        // Validate CORS configuration
        var (corsValid, corsError) = _corsOptions.Validate();
        if (!corsValid)
        {
            _logger.LogError("CORS configuration validation failed: {Error}", corsError);
            throw new InvalidOperationException($"CORS configuration is invalid: {corsError}");
        }

        // Validate Brevo configuration (if enabled)
        if (_brevoOptions?.Enabled ?? false)
        {
            var (brevoValid, brevoError) = _brevoOptions.Validate();
            if (!brevoValid)
            {
                _logger.LogError("Brevo configuration validation failed: {Error}", brevoError);
                throw new InvalidOperationException($"Brevo configuration is invalid: {brevoError}");
            }
        }

        // Validate Cloudinary configuration (if enabled)
        if (_cloudinaryOptions?.Enabled ?? false)
        {
            var (cloudinaryValid, cloudinaryError) = _cloudinaryOptions.Validate();
            if (!cloudinaryValid)
            {
                _logger.LogError("Cloudinary configuration validation failed: {Error}", cloudinaryError);
                throw new InvalidOperationException($"Cloudinary configuration is invalid: {cloudinaryError}");
            }
        }

        // Validate Razorpay configuration (if enabled)
        if (_razorpayOptions?.Enabled ?? false)
        {
            var (razorpayValid, razorpayError) = _razorpayOptions.Validate();
            if (!razorpayValid)
            {
                _logger.LogError("Razorpay configuration validation failed: {Error}", razorpayError);
                throw new InvalidOperationException($"Razorpay configuration is invalid: {razorpayError}");
            }
        }

        // Log loaded configuration (excluding sensitive data)
        LogConfiguration();

        _logger.LogInformation("Platform configuration validation passed");
    }

    /// <summary>
    /// Logs the loaded platform configuration for troubleshooting.
    /// </summary>
    private void LogConfiguration()
    {
        _logger.LogInformation("=== Platform Configuration ===");

        _logger.LogInformation(
            "Reserved Subdomains: {ReservedCount} ({Subdomains})",
            _multiTenancyOptions.ParsedReservedSubdomains.Count,
            string.Join(", ", _multiTenancyOptions.ParsedReservedSubdomains));

        _logger.LogInformation(
            "Excluded Subdomains: {ExcludedCount} ({Subdomains})",
            _multiTenancyOptions.ParsedExcludedSubdomains.Count,
            string.Join(", ", _multiTenancyOptions.ParsedExcludedSubdomains));

        _logger.LogInformation(
            "Allowed CORS Origins: {OriginCount} ({Origins})",
            _corsOptions.ParsedAllowedOrigins.Count,
            string.Join(", ", _corsOptions.ParsedAllowedOrigins));

        // Log external service status (without exposing secrets)
        _logger.LogInformation(
            "Brevo Email Service: {Enabled}",
            _brevoOptions?.Enabled ?? false ? "Enabled" : "Disabled");

        _logger.LogInformation(
            "Cloudinary Media Service: {Enabled}",
            _cloudinaryOptions?.Enabled ?? false ? "Enabled" : "Disabled");

        _logger.LogInformation(
            "Razorpay Payment Gateway: {Enabled}",
            _razorpayOptions?.Enabled ?? false ? "Enabled" : "Disabled");

        _logger.LogInformation("=== End Configuration ===");
    }
}
