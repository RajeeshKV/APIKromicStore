using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.UpdatePaymentSettings;

public sealed class UpdatePaymentSettingsCommand : IRequest<UpdatePaymentSettingsResponse>
{
    public Guid TenantId { get; set; }
    public bool RazorpayEnabled { get; set; }
    public string? RazorpayKeyId { get; set; }
    public string? RazorpayKeySecret { get; set; }
}

public sealed class UpdatePaymentSettingsResponse
{
    public Guid TenantId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
