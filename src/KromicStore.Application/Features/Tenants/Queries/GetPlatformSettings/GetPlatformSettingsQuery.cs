using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetPlatformSettings;

public sealed class GetPlatformSettingsQuery : IRequest<PlatformSettingsDto>
{
}

public sealed class PlatformSettingsDto
{
    public string PlatformName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string SupportEmail { get; set; } = string.Empty;
    public string? SupportPhoneNumber { get; set; }
    public string DefaultCurrency { get; set; } = string.Empty;
    public string DefaultTimezone { get; set; } = string.Empty;
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public bool AllowNewTenantSignups { get; set; }
    public bool RequireEmailVerification { get; set; }
}
