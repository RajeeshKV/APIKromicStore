using KromicStore.Domain.Promotions.Entities;

namespace KromicStore.Application.Features.Promotions.Abstractions;

public interface IPromotionRepository
{
    // Coupon methods
    Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Coupon?> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Coupon>> GetActiveCouponsAsync(CancellationToken cancellationToken = default);
    Task<List<Coupon>> GetValidCouponsAsync(CancellationToken cancellationToken = default);
    Task<bool> CouponExistsAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Discount methods
    Task<Discount?> GetDiscountByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Discount>> GetAllDiscountsAsync(CancellationToken cancellationToken = default);
    Task<List<Discount>> GetValidDiscountsAsync(CancellationToken cancellationToken = default);
    Task<List<Discount>> GetDiscountsByTypeAsync(DiscountType type, CancellationToken cancellationToken = default);
    Task<bool> DiscountExistsAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Campaign methods
    Task<Campaign?> GetCampaignByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Campaign>> GetAllCampaignsAsync(CancellationToken cancellationToken = default);
    Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default);
    Task<List<Campaign>> GetValidCampaignsAsync(CancellationToken cancellationToken = default);
    Task<bool> CampaignExistsAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Data modification
    void AddCoupon(Coupon coupon);
    void UpdateCoupon(Coupon coupon);
    void DeleteCoupon(Coupon coupon);
    
    void AddDiscount(Discount discount);
    void UpdateDiscount(Discount discount);
    void DeleteDiscount(Discount discount);
    
    void AddCampaign(Campaign campaign);
    void UpdateCampaign(Campaign campaign);
    void DeleteCampaign(Campaign campaign);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
