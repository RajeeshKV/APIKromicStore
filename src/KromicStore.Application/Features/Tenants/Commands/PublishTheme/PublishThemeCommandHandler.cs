using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Commands.PublishTheme;

public sealed class PublishThemeCommandHandler : IRequestHandler<PublishThemeCommand, Unit>
{
    private readonly IThemeRepository _themeRepository;
    private readonly ILogger<PublishThemeCommandHandler> _logger;

    public PublishThemeCommandHandler(IThemeRepository themeRepository, ILogger<PublishThemeCommandHandler> logger)
    {
        _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(PublishThemeCommand request, CancellationToken cancellationToken)
    {
        if (request.ThemeId == Guid.Empty)
            throw new ArgumentException("Theme ID is required.", nameof(request.ThemeId));

        var theme = await _themeRepository.GetByIdAsync(request.ThemeId, cancellationToken);
        if (theme == null)
            throw new InvalidOperationException($"Theme {request.ThemeId} not found.");

        theme.Publish();
        _themeRepository.Update(theme);
        await _themeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Theme {ThemeId} published", request.ThemeId);
        return Unit.Value;
    }
}
