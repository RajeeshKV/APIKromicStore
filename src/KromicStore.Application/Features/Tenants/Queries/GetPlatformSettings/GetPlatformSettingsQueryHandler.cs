using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetPlatformSettings;

public sealed class GetPlatformSettingsQueryHandler : IRequestHandler<GetPlatformSettingsQuery, PlatformSettingsDto>
{
    private readonly IPlatformSettingsRepository _settingsRepository;
    private readonly ILogger<GetPlatformSettingsQueryHandler> _logger;

    public GetPlatformSettingsQueryHandler(
        IPlatformSettingsRepository settingsRepository,
        ILogger<GetPlatformSettingsQueryHandler> logger)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PlatformSettingsDto> Handle(GetPlatformSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving platform settings");

        var settings = await _settingsRepository.GetAsync(cancellationToken);
        if (settings == null)
            throw new InvalidOperationException("Platform settings not found.");

        return new PlatformSettingsDto
        {
            PlatformName = settings.PlatformName,
            LogoUrl = settings.LogoUrl,
            FaviconUrl = settings.FaviconUrl,
            SupportEmail = settings.SupportEmail,
            SupportPhoneNumber = settings.SupportPhoneNumber,
            DefaultCurrency = settings.DefaultCurrency,
            DefaultTimezone = settings.DefaultTimezone,
            MaintenanceMode = settings.MaintenanceMode,
            MaintenanceMessage = settings.MaintenanceMessage,
            AllowNewTenantSignups = settings.AllowNewTenantSignups,
            RequireEmailVerification = settings.RequireEmailVerification
        };
    }
}
