using MediatR;

namespace KromicStore.Application.Features.Taxes.Commands.CreateTaxRule;

public sealed class CreateTaxRuleCommand : IRequest<CreateTaxRuleResponse>
{
    public Guid TaxRegionId { get; set; }
    public string ProductCategory { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public string? Description { get; set; }
    public DateTime? EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
}

public sealed class CreateTaxRuleResponse
{
    public Guid RuleId { get; set; }
    public string ProductCategory { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; }
}
