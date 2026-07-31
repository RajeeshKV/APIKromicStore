using KromicStore.Application.Features.Taxes.Abstractions;
using KromicStore.Domain.Taxes.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

public class TaxRegionRepository : ITaxRegionRepository
{
    private readonly KromicStoreDbContext _dbContext;

    public TaxRegionRepository(KromicStoreDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<TaxRegion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaxRegions.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<TaxRegion>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaxRegions.ToListAsync(cancellationToken);
    }

    public async Task<List<TaxRegion>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaxRegions.Where(r => r.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<TaxRegion?> GetByCountryAndStateAsync(string countryCode, string? stateCode = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TaxRegions.Where(r => r.CountryCode == countryCode.ToUpperInvariant());
        
        if (string.IsNullOrEmpty(stateCode))
        {
            return await query.FirstOrDefaultAsync(r => r.StateCode == null, cancellationToken);
        }
        
        return await query.FirstOrDefaultAsync(r => r.StateCode == stateCode.ToUpperInvariant(), cancellationToken);
    }

    public async Task<List<TaxRule>> GetTaxRulesByRegionAsync(Guid regionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaxRules
            .Where(r => r.TaxRegionId == regionId && r.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaxRule?> GetTaxRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaxRules.FirstOrDefaultAsync(r => r.Id == ruleId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaxRegions.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public void Add(TaxRegion region)
    {
        _dbContext.TaxRegionSet.Add(region);
    }

    public void Update(TaxRegion region)
    {
        _dbContext.TaxRegionSet.Update(region);
    }

    public void Delete(TaxRegion region)
    {
        _dbContext.TaxRegionSet.Remove(region);
    }

    public void AddRule(TaxRule rule)
    {
        _dbContext.TaxRuleSet.Add(rule);
    }

    public void UpdateRule(TaxRule rule)
    {
        _dbContext.TaxRuleSet.Update(rule);
    }

    public void DeleteRule(TaxRule rule)
    {
        _dbContext.TaxRuleSet.Remove(rule);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
