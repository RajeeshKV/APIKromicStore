using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateSubscriptionPlan;

public sealed class UpdateSubscriptionPlanCommand : IRequest<UpdateSubscriptionPlanResponse>
{
    public Guid PlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal? AnnualPrice { get; set; }
    public int MaxProducts { get; set; }
    public int MaxCategories { get; set; }
}

public sealed class UpdateSubscriptionPlanResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
