using MediatR;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Commands.CreateFeatureFlag;

public sealed class CreateFeatureFlagCommand : IRequest<Unit>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public FeatureFlagScope Scope { get; set; } = FeatureFlagScope.Global;
}
