using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.CreateSubscriptionPlan;

public sealed class CreateSubscriptionPlanCommand : IRequest<CreateSubscriptionPlanResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal? AnnualPrice { get; set; }
    public int MaxProducts { get; set; } = 100;
    public int MaxCategories { get; set; } = 20;
    public int MaxCollections { get; set; } = 10;
    public int MaxStaff { get; set; } = 5;
    public long MaxStorageBytes { get; set; }
    public int MaxEmailsPerMonth { get; set; }
    public bool CanCustomizeDomain { get; set; }
    public bool CanUseCustomTheme { get; set; }
}

public sealed class CreateSubscriptionPlanResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
