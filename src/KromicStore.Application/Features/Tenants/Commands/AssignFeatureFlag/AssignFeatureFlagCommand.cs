using MediatR;
using KromicStore.Domain.Tenants;

namespace KromicStore.Application.Features.Tenants.Commands.AssignFeatureFlag;

public sealed class AssignFeatureFlagCommand : IRequest<Unit>
{
    public Guid FeatureFlagId { get; set; }
    public FeatureFlagAssignmentType AssignmentType { get; set; }
    public Guid AssignedToEntityId { get; set; }
    public bool IsEnabled { get; set; } = true;
}
