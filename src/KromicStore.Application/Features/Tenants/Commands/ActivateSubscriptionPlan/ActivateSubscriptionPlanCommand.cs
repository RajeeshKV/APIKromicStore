using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.ActivateSubscriptionPlan;

public sealed class ActivateSubscriptionPlanCommand : IRequest<Unit>
{
    public Guid PlanId { get; set; }
}
