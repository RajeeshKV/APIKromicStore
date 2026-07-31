using KromicStore.Domain.Taxes.Entities;

namespace KromicStore.Application.Features.Taxes.Abstractions;

public interface ITaxRegionRepository
{
    Task<TaxRegion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TaxRegion>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TaxRegion>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<TaxRegion?> GetByCountryAndStateAsync(string countryCode, string? stateCode = null, CancellationToken cancellationToken = default);
    Task<List<TaxRule>> GetTaxRulesByRegionAsync(Guid regionId, CancellationToken cancellationToken = default);
    Task<TaxRule?> GetTaxRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(TaxRegion region);
    void Update(TaxRegion region);
    void Delete(TaxRegion region);
    void AddRule(TaxRule rule);
    void UpdateRule(TaxRule rule);
    void DeleteRule(TaxRule rule);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
