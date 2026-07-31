using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.UpdatePlatformSettings;

public sealed class UpdatePlatformSettingsCommandHandler : IRequestHandler<UpdatePlatformSettingsCommand, Unit>
{
    private readonly IPlatformSettingsRepository _settingsRepository;
    private readonly ILogger<UpdatePlatformSettingsCommandHandler> _logger;

    public UpdatePlatformSettingsCommandHandler(
        IPlatformSettingsRepository settingsRepository,
        ILogger<UpdatePlatformSettingsCommandHandler> logger)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(UpdatePlatformSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating platform settings");

        var settings = await _settingsRepository.GetAsync(cancellationToken);
        if (settings == null)
            throw new InvalidOperationException("Platform settings not found.");

        if (!string.IsNullOrWhiteSpace(request.PlatformName) || !string.IsNullOrWhiteSpace(request.SupportEmail))
        {
            settings.UpdateGeneralSettings(
                request.PlatformName ?? settings.PlatformName,
                request.LogoUrl ?? settings.LogoUrl,
                request.FaviconUrl ?? settings.FaviconUrl);
        }

        if (!string.IsNullOrWhiteSpace(request.SupportEmail) || !string.IsNullOrWhiteSpace(request.DefaultCurrency))
        {
            settings.UpdateContactSettings(
                request.SupportEmail ?? settings.SupportEmail,
                request.SupportPhoneNumber ?? settings.SupportPhoneNumber,
                null);

            settings.UpdateDefaultSettings(
                request.DefaultCurrency ?? settings.DefaultCurrency,
                request.DefaultTimezone ?? settings.DefaultTimezone);
        }

        if (request.MaintenanceMode.HasValue)
        {
            settings.SetMaintenanceMode(request.MaintenanceMode.Value, request.MaintenanceMessage);
        }

        if (request.AllowNewTenantSignups.HasValue || request.RequireEmailVerification.HasValue)
        {
            settings.UpdateSignupSettings(
                request.AllowNewTenantSignups ?? settings.AllowNewTenantSignups,
                settings.AllowTrialSignups,
                request.RequireEmailVerification ?? settings.RequireEmailVerification,
                settings.RequireManualApproval);
        }

        _settingsRepository.Update(settings);
        await _settingsRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Platform settings updated successfully");
        return Unit.Value;
    }
}
