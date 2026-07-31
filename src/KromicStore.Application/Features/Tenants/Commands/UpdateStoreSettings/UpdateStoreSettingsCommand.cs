using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateStoreSettings;

public sealed class UpdateStoreSettingsCommand : IRequest<UpdateStoreSettingsResponse>
{
    public Guid TenantId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Address { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Timezone { get; set; }
    public string? Language { get; set; }
}

public sealed class UpdateStoreSettingsResponse
{
    public Guid TenantId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
