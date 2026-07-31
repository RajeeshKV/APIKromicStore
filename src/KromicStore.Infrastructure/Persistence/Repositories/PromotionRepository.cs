using KromicStore.Application.Features.Promotions.Abstractions;
using KromicStore.Domain.Promotions.Entities;
using Microsoft.EntityFrameworkCore;

namespace KromicStore.Infrastructure.Persistence.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly KromicStoreDbContext _dbContext;

    public PromotionRepository(KromicStoreDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    // Coupon methods
    public async Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Coupons
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<Coupon?> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Coupon>> GetActiveCouponsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Coupons
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Coupon>> GetValidCouponsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.Coupons
            .Where(c => c.IsActive && c.ValidFromUtc <= now && now <= c.ValidToUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CouponExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Coupons.AnyAsync(c => c.Id == id, cancellationToken);
    }

    // Discount methods
    public async Task<Discount?> GetDiscountByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Discounts.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<List<Discount>> GetAllDiscountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Discounts.ToListAsync(cancellationToken);
    }

    public async Task<List<Discount>> GetValidDiscountsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.Discounts
            .Where(d => d.IsActive && d.ValidFromUtc <= now && now <= d.ValidToUtc)
            .OrderBy(d => d.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Discount>> GetDiscountsByTypeAsync(DiscountType type, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Discounts
            .Where(d => d.Type == type && d.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DiscountExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Discounts.AnyAsync(d => d.Id == id, cancellationToken);
    }

    // Campaign methods
    public async Task<Campaign?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Campaign>> GetAllCampaignsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Campaigns.ToListAsync(cancellationToken);
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Campaigns
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Campaign>> GetValidCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.Campaigns
            .Where(c => c.IsActive && c.ValidFromUtc <= now && now <= c.ValidToUtc)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CampaignExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Campaigns.AnyAsync(c => c.Id == id, cancellationToken);
    }

    // Data modification
    public void AddCoupon(Coupon coupon)
    {
        _dbContext.CouponSet.Add(coupon);
    }

    public void UpdateCoupon(Coupon coupon)
    {
        _dbContext.CouponSet.Update(coupon);
    }

    public void DeleteCoupon(Coupon coupon)
    {
        _dbContext.CouponSet.Remove(coupon);
    }

    public void AddDiscount(Discount discount)
    {
        _dbContext.DiscountSet.Add(discount);
    }

    public void UpdateDiscount(Discount discount)
    {
        _dbContext.DiscountSet.Update(discount);
    }

    public void DeleteDiscount(Discount discount)
    {
        _dbContext.DiscountSet.Remove(discount);
    }

    public void AddCampaign(Campaign campaign)
    {
        _dbContext.CampaignSet.Add(campaign);
    }

    public void UpdateCampaign(Campaign campaign)
    {
        _dbContext.CampaignSet.Update(campaign);
    }

    public void DeleteCampaign(Campaign campaign)
    {
        _dbContext.CampaignSet.Remove(campaign);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
