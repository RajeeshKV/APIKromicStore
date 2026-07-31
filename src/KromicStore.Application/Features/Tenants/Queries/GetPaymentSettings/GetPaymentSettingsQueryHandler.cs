using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetPaymentSettings;

public sealed class GetPaymentSettingsQueryHandler : IRequestHandler<GetPaymentSettingsQuery, PaymentSettingsResponse>
{
    private readonly IPlatformSettingsRepository _settingsRepository;
    private readonly ILogger<GetPaymentSettingsQueryHandler> _logger;

    public GetPaymentSettingsQueryHandler(
        IPlatformSettingsRepository settingsRepository,
        ILogger<GetPaymentSettingsQueryHandler> logger)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentSettingsResponse> Handle(GetPaymentSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving payment settings for tenant {TenantId}", request.TenantId);

        // Load platform settings which contain payment configuration
        var platformSettings = await _settingsRepository.GetAsync(cancellationToken);
        
        if (platformSettings == null)
        {
            _logger.LogWarning("Platform settings not configured. Returning default payment settings.");
            return CreateDefaultPaymentSettings(request.TenantId);
        }

        // Extract payment settings from platform configuration
        return new PaymentSettingsResponse
        {
            TenantId = request.TenantId,
            RazorpayEnabled = platformSettings.RazorpayEnabled,
            RazorpayKeyId = platformSettings.RazorpayKeyId,
            SupportedPaymentMethods = new Dictionary<string, object>
            {
                { "razorpay", new { enabled = platformSettings.RazorpayEnabled } },
                { "stripe", new { enabled = platformSettings.StripeEnabled ?? false } },
                { "paypal", new { enabled = platformSettings.PayPalEnabled ?? false } }
            }
        };
    }

    private static PaymentSettingsResponse CreateDefaultPaymentSettings(Guid tenantId)
    {
        return new PaymentSettingsResponse
        {
            TenantId = tenantId,
            RazorpayEnabled = false,
            RazorpayKeyId = null,
            SupportedPaymentMethods = new Dictionary<string, object>
            {
                { "razorpay", new { enabled = false } },
                { "stripe", new { enabled = false } },
                { "paypal", new { enabled = false } }
            }
        };
    }
}
