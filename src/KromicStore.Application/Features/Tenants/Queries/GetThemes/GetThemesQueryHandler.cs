using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Queries.GetThemes;

public sealed class GetThemesQueryHandler : IRequestHandler<GetThemesQuery, GetThemesResponse>
{
    private readonly IThemeRepository _themeRepository;
    private readonly ILogger<GetThemesQueryHandler> _logger;

    public GetThemesQueryHandler(IThemeRepository themeRepository, ILogger<GetThemesQueryHandler> logger)
    {
        _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetThemesResponse> Handle(GetThemesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving themes: Skip={Skip}, Take={Take}, PublishedOnly={PublishedOnly}",
            request.Skip, request.Take, request.PublishedOnly);

        ThemeStatus? statusFilter = request.PublishedOnly == true ? ThemeStatus.Published : null;

        var (themes, totalCount) = await _themeRepository.GetPaginatedAsync(
            request.Skip, request.Take, statusFilter, null, cancellationToken);

        var dtos = themes.Select(t => new ThemeDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            Description = t.Description,
            PreviewImageUrl = t.PreviewImageUrl,
            IsPublished = t.IsPublished,
            TimesUsed = t.TimesUsed,
            CreatedOnUtc = t.CreatedOnUtc
        }).ToList();

        return new GetThemesResponse { Themes = dtos, TotalCount = totalCount };
    }
}
