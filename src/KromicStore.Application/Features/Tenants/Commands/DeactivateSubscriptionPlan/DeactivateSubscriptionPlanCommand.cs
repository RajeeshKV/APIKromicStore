using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.DeactivateSubscriptionPlan;

public sealed class DeactivateSubscriptionPlanCommand : IRequest<Unit>
{
    public Guid PlanId { get; set; }
}
