using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.UpdatePlatformSettings;

public sealed class UpdatePlatformSettingsCommand : IRequest<Unit>
{
    public string? PlatformName { get; set; }
    public string? SupportEmail { get; set; }
    public string? SupportPhoneNumber { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? DefaultCurrency { get; set; }
    public string? DefaultTimezone { get; set; }
    public bool? AllowNewTenantSignups { get; set; }
    public bool? RequireEmailVerification { get; set; }
    public bool? MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
}
