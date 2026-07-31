using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Taxes.Abstractions;
using KromicStore.Domain.Taxes.Entities;
using MediatR;

namespace KromicStore.Application.Features.Taxes.Commands.CreateTaxRule;

public sealed class CreateTaxRuleCommandHandler : IRequestHandler<CreateTaxRuleCommand, CreateTaxRuleResponse>
{
    private readonly ITaxRegionRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateTaxRuleCommandHandler(ITaxRegionRepository repository, ITenantContext tenantContext)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<CreateTaxRuleResponse> Handle(CreateTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required");
        
        // Verify tax region exists
        var region = await _repository.GetByIdAsync(request.TaxRegionId, cancellationToken);
        if (region == null)
            throw new InvalidOperationException($"Tax region {request.TaxRegionId} not found");
        
        // Create tax rule
        var rule = TaxRule.Create(
            tenantId,
            request.TaxRegionId,
            request.ProductCategory,
            request.TaxRate,
            request.Description,
            request.EffectiveFromUtc,
            request.EffectiveToUtc);
        
        // Add to repository
        _repository.AddRule(rule);
        await _repository.SaveChangesAsync(cancellationToken);
        
        return new CreateTaxRuleResponse
        {
            RuleId = rule.Id,
            ProductCategory = rule.ProductCategory,
            TaxRate = rule.TaxRate,
            IsActive = rule.IsActive
        };
    }
}
