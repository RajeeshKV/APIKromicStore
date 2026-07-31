using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Commands.CreateTheme;

public sealed class CreateThemeCommandHandler : IRequestHandler<CreateThemeCommand, CreateThemeResponse>
{
    private readonly IThemeRepository _themeRepository;
    private readonly ILogger<CreateThemeCommandHandler> _logger;

    public CreateThemeCommandHandler(IThemeRepository themeRepository, ILogger<CreateThemeCommandHandler> logger)
    {
        _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateThemeResponse> Handle(CreateThemeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Theme name is required.", nameof(request.Name));
        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Theme slug is required.", nameof(request.Slug));

        _logger.LogInformation("Creating theme: {ThemeName}", request.Name);

        var theme = Theme.Create(request.Name, request.Slug, request.Description, request.PreviewImageUrl);
        _themeRepository.Add(theme);
        await _themeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Theme {ThemeId} created successfully", theme.Id);

        return new CreateThemeResponse { Id = theme.Id, Name = theme.Name };
    }
}
