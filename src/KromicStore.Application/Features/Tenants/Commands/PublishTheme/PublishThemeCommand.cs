using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.PublishTheme;

public sealed class PublishThemeCommand : IRequest<Unit>
{
    public Guid ThemeId { get; set; }
}
