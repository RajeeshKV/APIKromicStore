using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetSubscriptionPlans;

public sealed class GetSubscriptionPlansQuery : IRequest<SubscriptionPlansResponse>
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public bool? IsActive { get; set; }
}

public sealed class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal? AnnualPrice { get; set; }
    public int MaxProducts { get; set; }
    public int MaxCategories { get; set; }
    public int MaxStaff { get; set; }
    public bool IsActive { get; set; }
}

public sealed class SubscriptionPlansResponse
{
    public List<SubscriptionPlanDto> Plans { get; set; } = new();
    public int TotalCount { get; set; }
}
