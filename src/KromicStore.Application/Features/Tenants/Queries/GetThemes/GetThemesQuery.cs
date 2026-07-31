using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetThemes;

public sealed class GetThemesQuery : IRequest<GetThemesResponse>
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public bool? PublishedOnly { get; set; }
}

public sealed class ThemeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public int TimesUsed { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}

public sealed class GetThemesResponse
{
    public List<ThemeDto> Themes { get; set; } = new();
    public int TotalCount { get; set; }
}
