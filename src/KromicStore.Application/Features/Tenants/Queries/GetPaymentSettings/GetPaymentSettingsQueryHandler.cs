using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Queries.GetPaymentSettings;

public sealed class GetPaymentSettingsQueryHandler : IRequestHandler<GetPaymentSettingsQuery, PaymentSettingsResponse>
{
    private readonly ILogger<GetPaymentSettingsQueryHandler> _logger;

    public GetPaymentSettingsQueryHandler(ILogger<GetPaymentSettingsQueryHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentSettingsResponse> Handle(GetPaymentSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving payment settings for tenant {TenantId}", request.TenantId);

        // TODO: Inject ITenantSettingsRepository when available to load real payment settings
        // For now, return default response
        return await Task.FromResult(new PaymentSettingsResponse
        {
            TenantId = request.TenantId,
            RazorpayEnabled = false,
            RazorpayKeyId = null,
            SupportedPaymentMethods = new Dictionary<string, object>
            {
                { "razorpay", new { enabled = false } },
                { "stripe", new { enabled = false } },
                { "paypal", new { enabled = false } }
            }
        });
    }
}
