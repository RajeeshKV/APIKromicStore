using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetPaymentSettings;

public sealed class GetPaymentSettingsQuery : IRequest<PaymentSettingsResponse>
{
    public Guid TenantId { get; set; }
}

public sealed class PaymentSettingsResponse
{
    public Guid TenantId { get; set; }
    public bool RazorpayEnabled { get; set; }
    public string? RazorpayKeyId { get; set; }
    // RazorpayKeySecret is NOT returned for security
    public Dictionary<string, object> SupportedPaymentMethods { get; set; } = new();
}
