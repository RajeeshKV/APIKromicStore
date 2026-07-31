using KromicStore.Application.Features.Promotions.Abstractions;
using MediatR;

namespace KromicStore.Application.Features.Promotions.Commands.ApplyCoupon;

public sealed class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, ApplyCouponResponse>
{
    private readonly IPromotionRepository _repository;

    public ApplyCouponCommandHandler(IPromotionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ApplyCouponResponse> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        // Get coupon by code
        var coupon = await _repository.GetCouponByCodeAsync(request.CouponCode, cancellationToken);
        
        if (coupon == null)
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = "Coupon code not found"
            };
        
        // Check if coupon can be used
        if (!coupon.CanBeUsed())
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = "Coupon is expired or no longer valid"
            };
        
        // Check minimum order value
        if (coupon.MinimumOrderValue.HasValue && request.OrderAmount < coupon.MinimumOrderValue)
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = $"Minimum order value of {coupon.MinimumOrderValue} required"
            };
        
        // Get discount
        var discount = await _repository.GetDiscountByIdAsync(coupon.DiscountId, cancellationToken);
        if (discount == null)
            return new ApplyCouponResponse
            {
                IsValid = false,
                Message = "Associated discount not found"
            };
        
        // Calculate discount amount
        var discountAmount = discount.CalculateDiscountAmount(request.OrderAmount);
        
        return new ApplyCouponResponse
        {
            IsValid = true,
            Message = "Coupon applied successfully",
            DiscountId = coupon.DiscountId,
            DiscountAmount = discountAmount
        };
    }
}
