using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.DeleteSubscriptionPlan;

public sealed class DeleteSubscriptionPlanCommand : IRequest<Unit>
{
    public Guid PlanId { get; set; }
}
