using MediatR;

namespace KromicStore.Application.Features.Promotions.Commands.ApplyCoupon;

public sealed class ApplyCouponCommand : IRequest<ApplyCouponResponse>
{
    public string CouponCode { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public decimal OrderAmount { get; set; }
}

public sealed class ApplyCouponResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? DiscountId { get; set; }
    public decimal DiscountAmount { get; set; }
}
