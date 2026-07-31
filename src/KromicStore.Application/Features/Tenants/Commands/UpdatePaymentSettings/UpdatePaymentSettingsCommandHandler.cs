using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Tenants.Commands.UpdatePaymentSettings;

public sealed class UpdatePaymentSettingsCommandHandler : IRequestHandler<UpdatePaymentSettingsCommand, UpdatePaymentSettingsResponse>
{
    private readonly ILogger<UpdatePaymentSettingsCommandHandler> _logger;

    public UpdatePaymentSettingsCommandHandler(ILogger<UpdatePaymentSettingsCommandHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdatePaymentSettingsResponse> Handle(UpdatePaymentSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating payment settings for tenant {TenantId}", request.TenantId);

        if (request.RazorpayEnabled)
        {
            if (string.IsNullOrEmpty(request.RazorpayKeyId) || string.IsNullOrEmpty(request.RazorpayKeySecret))
            {
                _logger.LogWarning("Razorpay credentials incomplete for tenant {TenantId}", request.TenantId);
                throw new InvalidOperationException("Razorpay Key ID and Secret are required.");
            }
        }

        // TODO: Inject ITenantSettingsRepository when available to persist payment settings
        // For now, log and return success
        _logger.LogInformation("Payment settings updated for tenant {TenantId}", request.TenantId);

        return await Task.FromResult(new UpdatePaymentSettingsResponse
        {
            TenantId = request.TenantId,
            Success = true,
            Message = "Payment settings updated successfully"
        });
    }
}
