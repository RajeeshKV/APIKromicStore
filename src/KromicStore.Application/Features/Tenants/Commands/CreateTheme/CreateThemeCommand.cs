using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.CreateTheme;

public sealed class CreateThemeCommand : IRequest<CreateThemeResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviewImageUrl { get; set; }
}

public sealed class CreateThemeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
